using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Security.Cryptography.Xml;

namespace DotnetSample
{
    class Program
    {
        private static readonly byte[] bytesToHash =
            new byte[]
            {
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
                0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
            };

        static void Main(string[] args)
        {
            var hash = HashAlgorithmName.Gost3411_2012_256;
            Program.MakeMeCert();
            var gostCert = new X509Certificate2(@"MyCert.pfx", "1");
            var gostSk = gostCert.PrivateKey as Gost3410_2012_256CryptoServiceProvider;
            var signed = gostSk.SignData(bytesToHash, hash);

            var gostPk = gostCert.GetGost3410_2012_256PublicKey();
            var dataValidationResult = gostPk.VerifyData(bytesToHash, signed, hash);

            Console.WriteLine($"Validation result: {dataValidationResult}");

            Program.CreateSomeXml("test.txml");
            Program.SignXmlFile("test.txml", "test.xml.sig", gostSk, gostCert);
            Program.ValidateXmlFIle("test.xml.sig");
        }

        /// <summary>
        /// Создать сертификат ГОСТ 34.10.2012 256
        /// </summary>
        private static void MakeMeCert()
        {
            using (var parent = new Gost3410_2012_256CryptoServiceProvider())
            {
                var parentReq = new CertificateRequest(
                    "CN=Experimental Issuing Authority",
                    parent,
                    HashAlgorithmName.Gost3411_2012_256);

                parentReq.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(true, false, 0, true));

                parentReq.CertificateExtensions.Add(
                    new X509SubjectKeyIdentifierExtension(parentReq.PublicKey, false));

                using (var parentCert = parentReq.CreateSelfSigned(
                    DateTimeOffset.UtcNow.AddDays(-45),
                    DateTimeOffset.UtcNow.AddDays(365)))
                {
                    byte[] certData = parentCert.Export(X509ContentType.Pfx, "1");
                    File.WriteAllBytes(@"MyCert.pfx", certData);
                }
            }
        }

        static void CreateSomeXml(string FileName)
        {
            string SourceDocument = "" +
            "<MyXML Signed=\"true\">" +
            "    <ElementToSign Signed=\"true\">" +
                    "Here is some data to sign.</ElementToSign>" +
            "</MyXML>";

            // Создать документ по строке
            XmlDocument document = new XmlDocument();
            document.LoadXml(SourceDocument);

            // Сохранить подписываемый документ в файле.
            using (XmlTextWriter xmltw = new XmlTextWriter(FileName,
                new UTF8Encoding(false)))
            {
                xmltw.WriteStartDocument();
                document.WriteTo(xmltw);
            }
        }

        /// <summary>
        /// Подпись xml файла
        /// </summary>
        static void SignXmlFile(string FileName,
            string SignedFileName, AsymmetricAlgorithm Key,
            X509Certificate Certificate)
        {
            // Создаем новый XML документ.
            XmlDocument doc = new XmlDocument();

            // Пробельные символы участвуют в вычислении подписи и должны быть сохранены для совместимости с другими реализациями.
            doc.PreserveWhitespace = true;

            // Читаем документ из файла.
            doc.Load(new XmlTextReader(FileName));

            // Создаем объект SignedXml по XML документу.
            SignedXml signedXml = new SignedXml(doc);

            // Добавляем ключ в SignedXml документ. 
            signedXml.SigningKey = Key;

            // Создаем ссылку на node для подписи.
            // При подписи всего документа проставляем "".
            Reference reference = new Reference();
            reference.Uri = "";

            // Явно проставляем алгоритм хэширования,
            // по умолчанию SHA1.
            reference.DigestMethod =
                SignedXml.XmlDsigGost3411_2012_256Url;

            // Добавляем transform на подписываемые данные
            // для удаления вложенной подписи.
            XmlDsigEnvelopedSignatureTransform env =
                new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Добавляем СМЭВ трансформ.
            // начиная с .NET 4.5.1 для проверки подписи, необходимо добавить этот трансформ в довернные:
            // signedXml.SafeCanonicalizationMethods.Add("urn://smev-gov-ru/xmldsig/transform");
            XmlDsigSmevTransform smev =
                new XmlDsigSmevTransform();
            reference.AddTransform(smev);

            // Добавляем transform для канонизации.
            XmlDsigC14NTransform c14 = new XmlDsigC14NTransform();
            reference.AddTransform(c14);

            // Добавляем ссылку на подписываемые данные
            signedXml.AddReference(reference);

            // Создаем объект KeyInfo.
            KeyInfo keyInfo = new KeyInfo();

            // Добавляем сертификат в KeyInfo
            keyInfo.AddClause(new KeyInfoX509Data(Certificate));

            // Добавляем KeyInfo в SignedXml.
            signedXml.KeyInfo = keyInfo;

            // Вычисляем подпись.
            signedXml.ComputeSignature();

            // Получаем XML представление подписи и сохраняем его 
            // в отдельном node.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Добавляем node подписи в XML документ.
            doc.DocumentElement.AppendChild(doc.ImportNode(
                xmlDigitalSignature, true));

            // При наличии стартовой XML декларации ее удаляем
            // (во избежание повторного сохранения)
            if (doc.FirstChild is XmlDeclaration)
            {
                doc.RemoveChild(doc.FirstChild);
            }

            // Сохраняем подписанный документ в файле.
            using (XmlTextWriter xmltw = new XmlTextWriter(SignedFileName,
                new UTF8Encoding(false)))
            {
                xmltw.WriteStartDocument();
                doc.WriteTo(xmltw);
            }
        }

        /// <summary>
        /// Проверка xml файла
        /// </summary>
        /// <param name="path"></param>
        static void ValidateXmlFIle(string path)
        {
            // Создаем новый XML документ в памяти.
            XmlDocument xmlDocument = new XmlDocument();

            // Сохраняем все пробельные символы, они важны при проверке 
            // подписи.
            xmlDocument.PreserveWhitespace = true;

            // Загружаем подписанный документ из файла.
            xmlDocument.Load(path);

            // Ищем все node "Signature" и сохраняем их в объекте XmlNodeList
            XmlNodeList nodeList = xmlDocument.GetElementsByTagName(
                "Signature", SignedXml.XmlDsigNamespaceUrl);

            Console.WriteLine("Найдено:{0} подпис(ей).", nodeList.Count);

            // Проверяем все подписи.
            for (int curSignature = 0; curSignature < nodeList.Count; curSignature++)
            {
                // Создаем объект SignedXml для проверки подписи документа.
                SignedXml signedXml = new SignedXml(xmlDocument);

                // начиная с .NET 4.5.1 для проверки подписи, необходимо добавить СМЭВ transform в довернные:

                signedXml.SafeCanonicalizationMethods.Add("urn://smev-gov-ru/xmldsig/transform");

                // Загружаем узел с подписью.
                signedXml.LoadXml((XmlElement)nodeList[curSignature]);

                // Проверяем подпись и выводим результат.
                bool result = signedXml.CheckSignature();

                // Выводим результат проверки подписи в консоль.
                if (result)
                {
                    Console.WriteLine("XML подпись[{0}] верна.", curSignature + 1);
                }
                else
                {
                    Console.WriteLine("XML подпись[{0}] не верна.", curSignature + 1);
                }
            }
        }
    }
}
