using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.Globalization;

namespace DotnetSample
{
    class Program
    {
        internal static readonly byte[] Gost2012_256Pfx = Program.HexToByteArray(
            "308204630201033082041F06092A864886F70D010701A08204100482040C3082" +
            "0408308201F106092A864886F70D010701A08201E2048201DE308201DA308201" +
            "D6060B2A864886F70D010C0A0102A081B73081B43024060A2A864886F70D010C" +
            "0150301604102B80933D90A0517A411C956079FD833B020207D004818BFE0081" +
            "EA331D75CEBF677827DBDD89A762E39BBA97504F92286581AFFEFB5FDF1DDED0" +
            "643066D4E7E3BD402D544AFCC00EA631158C34D61880268EAB2F93A0A70DC73A" +
            "791A6FA40DEF10D67463B792793A436B2F20066C41A103C61BD87927CFB70ABC" +
            "3E23D75701340ED3645970CAA09596843FB00271EC6CB49AE1F94CD90BFB63EA" +
            "66060066984F92B73182010B300D06092B06010401823711023100301306092A" +
            "864886F70D0109153106040401000000305D06092A864886F70D01091431501E" +
            "4E00740071002D00350066003900340031003300640037002D00320061003300" +
            "30002D0034006200620065002D0039006500660036002D006100330030006200" +
            "62003500320064003100330035003930818506092B060104018237110131781E" +
            "7600430072007900700074006F002D00500072006F00200047004F0053005400" +
            "200052002000330034002E00310030002D003200300031003200200043007200" +
            "7900700074006F00670072006100700068006900630020005300650072007600" +
            "6900630065002000500072006F007600690064006500723082020F06092A8648" +
            "86F70D010706A0820200308201FC020100308201F506092A864886F70D010701" +
            "301C060A2A864886F70D010C0103300E0408796F57043A7D1366020207D08082" +
            "01C8DD2DFAF95015D8D7BAF84B40561B556C271A4E4150DF340AA9AA1D0090E9" +
            "AFD299D670B28959E3B30EF66E02671A7565D76808B40976037BDB3FE37CD8BB" +
            "268D4F72EDD95538D8B549EC1380D988A945F2A4EDCB3A0E66AA48A06C9C2327" +
            "C7119813FB6B7A1CC8EA1E26AB7A6DCF8AF14CEC694E3F431420AF70545D7027" +
            "DE1F3D029A667F929BAF05C18DC0773AB49D32EFF34A8764670D4B65857FCF14" +
            "CF68FCA6EEC1172BA1A2604C4AF184927D5AF09AB8E4D57A461CB208403FBD46" +
            "61B4A3BC1D84DF7F96C54BD5FFEF2B1FC9964111F64182C74EB858132539B828" +
            "B72ACF7226E97FEF334DA49D064A592BC0038F01C925F51F9D8D66C275B48C69" +
            "C31B31D7A9313F4BEC29D54803783B93BB325885D030C6A416F9A11AEE36A6AF" +
            "85E04203D5EA176224C0F670B718123F3B82457156E4049CF1AF9B2B07E1D93A" +
            "62695EA1A9B3A6387D67E23E9119F19FF4C15EB911423F65CB35A8769CCAD9AC" +
            "ECAB8974742CBC0E9A4C7ECA746BDEA39CEF10F09662D3C3B405CF65746E11AF" +
            "70BDD74EF519551B833C0D15D42CA8BCE069C732A16B4A9954ED9D67A1520006" +
            "28E44E73F6F2807CF2B3F300879B258B52E8369B451DA3EF994D0A6AB6845DED" +
            "BB78ACF869AFFFFC0A36303B301F300706052B0E03021A04148ED177BCC23157" +
            "57BFAD832027CE64059E9FBD270414081BE6D39B6AEC3B424B72FB123AC2E461" +
            "D5CD4F020207D0");

        private static readonly byte[] bytesToHash =
            new byte[]
            {
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
                0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
            };

        static void Main(string[] args)
        {
            // загружаем сертификат из массива байт без окошек в in-memory store
            // контейнер для закрытого ключа не будет сохранён на машине
            using (var gostCert = new X509Certificate2(Gost2012_256Pfx, "1", X509KeyStorageFlags.CspNoPersistKeySet))
            {
                var hash = HashAlgorithmName.Gost3411_2012_256;
                var gostSk = gostCert.PrivateKey as Gost3410_2012_256CryptoServiceProvider;
                var signed = gostSk.SignData(bytesToHash, hash);
                var gostPk = gostCert.GetGost3410_2012_256PublicKey();
                var dataValidationResult = gostPk.VerifyData(bytesToHash, signed, hash);

                Console.WriteLine($"Validation result: {dataValidationResult}");

                Program.CreateSomeXml("test.txml");
                Program.SignXmlFile("test.txml", "test.xml.sig", gostSk, gostCert);
                Program.ValidateXmlFIle("test.xml.sig");
            }
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

        internal static byte[] HexToByteArray(string hexString)
        {
            byte[] bytes = new byte[hexString.Length / 2];

            for (int i = 0; i < hexString.Length; i += 2)
            {
                string s = hexString.Substring(i, 2);
                bytes[i / 2] = byte.Parse(s, NumberStyles.HexNumber, null);
            }

            return bytes;
        }
    }
}
