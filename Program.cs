using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;
using System.Text.Json;
using XmlParser;

internal class Program
{
    private static void Main(string[] args)
    {
        string xmlSuccess = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
            <s:Body>
                <ExportDailyInterfaceResponse xmlns=""http://www.giro.ca/OperationObjectInterfaceService2014"">
                    <ExportDailyInterfaceResult xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                        <OIGInterface>
                            <?xml version=""1.0"" encoding=""utf-8""?>
                            <object_interface xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
                                <vehicle>
                                    <veh_identifier>2001</veh_identifier>
                                    <veh_parked_garage>RH</veh_parked_garage>
                                    <veh_current_location>32 - 1</veh_current_location>
                                </vehicle>
                                <vehicle>
                                    <veh_identifier>2002</veh_identifier>
                                    <veh_parked_garage>RH</veh_parked_garage>
                                    <veh_current_location>32 - 2</veh_current_location>
                                </vehicle>
                            </object_interface>
                        </OIGInterface>
                        <ExportLog/>
                    </ExportDailyInterfaceResult>
                </ExportDailyInterfaceResponse>
            </s:Body>
        </s:Envelope>";
        string xmlError = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
            <s:Body>
                <ExportDailyInterfaceResponse xmlns=""http://www.giro.ca/OperationObjectInterfaceService2014"">
                    <ExportDailyInterfaceResult xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                        <OIGInterface>
                            <?xml version=""1.0"" encoding=""utf-8""?>
                            <object_interface xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""/>
                        </OIGInterface>
                        <ExportLog>
                            Daily schedule 13/09/2022 for division T-BEST does not exist.
                        </ExportLog>
                    </ExportDailyInterfaceResult>
                </ExportDailyInterfaceResponse>
            </s:Body>
        </s:Envelope>";

        // switch to test for success or error response
        var xml = xmlSuccess;
        // var xml = xmlError;

        // remove whitespace
        xml = xml.Replace("\n", "").Replace("\r", "");

        Match m = Regex.Match(xml, @"<ExportDailyInterfaceResult.*</ExportDailyInterfaceResult>");
        xml = m.Value;

        // remove xml tag
        xml = Regex.Replace(xml, @"<\?xml.*\?>", "");

        // get json string
        var xmlReader = XmlReader.Create(new StringReader(xml));
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlReader);
        var response = JsonConvert.SerializeXmlNode(xmlDoc);

        // parse json string
        using var jsonDoc = JsonDocument.Parse(response);
        JsonElement jsonElement = jsonDoc.RootElement;

        var object_interface = jsonElement.GetProperty("ExportDailyInterfaceResult").GetProperty("OIGInterface").GetProperty("object_interface");
        var export_log = jsonElement.GetProperty("ExportDailyInterfaceResult").GetProperty("ExportLog");

        // convert json to object
        if (export_log.ToString() == "")
        {
            Console.WriteLine("Got Success Response...");

            var vehicle = object_interface.GetProperty("vehicle");
            var vehicleList = JsonConvert.DeserializeObject<IList<Vehicle>>(vehicle.ToString());

            foreach (var v in vehicleList)
            {
                Console.WriteLine(v.veh_identifier);
                Console.WriteLine(v.veh_parked_garage);
                Console.WriteLine(v.veh_current_location);
            }
        }
        else
        {
            Console.WriteLine("Got Error...");
            Console.WriteLine(export_log);
        }
    }
}