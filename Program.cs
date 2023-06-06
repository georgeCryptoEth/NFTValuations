using System;
using System.Numerics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Demo2CSharp
{

    public class InputData
    {
        public int tokenId { get; set; }
        public String TokenIndeX { get; set; }
        public string ContractAddress { get; set; }
    }


    public class JsonInfuraClass
    {
        public string jsonrpc { get; set; }
        public int id { get; set; }
        public string result { get; set; }  // An integer value of the latest block number encoded as hexadecimal
        public override string ToString()
        {
            return $"[{jsonrpc}, {id}, {result}]";
        }
    }


    class Web3APIClass
    {

        public string api_key = "";

        public JsonInfuraClass GetJSONData(String action, String [] param)
        {
            if (action == "eth_blockNumber" || action == "eth_getBalance")
            {
                String url = "https://mainnet.infura.io/v3/" + api_key;

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";

                httpRequest.Accept = "application/json";
                httpRequest.ContentType = "application/json";

                String param_m = "";
                if (param.Length > 0) {
                     param_m = "\"" + string.Join("\",\"", param) + "\"";
                }

                //HANDCRAFTED JSON
                var data = "{\n                            \"jsonrpc\": \"2.0\",\n                            \"method\": \""+ action + "\",\n                            \"params\": ["+ param_m + "],\n                            \"id\": 1\n                            }";

                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                String jsonString = "";
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    jsonString = streamReader.ReadToEnd();
                }

                String r = httpResponse.StatusCode.ToString();
                JsonInfuraClass JsonInfuraClass_ref = JsonSerializer.Deserialize<JsonInfuraClass>(jsonString);

                return JsonInfuraClass_ref;

            }

            return new JsonInfuraClass();

        }
    }





    class MainClass
    {
        public static void Main(string[] args)
        {
            // get input data Object
            InputData ro = new InputData();

            using (StreamReader r = File.OpenText("../../input.json"))
            {
                string json = r.ReadToEnd();
                ro = JsonSerializer.Deserialize<InputData>(json);
            }

            Console.WriteLine(ro.ContractAddress);

            // Block API Owner
            var p = new Web3APIClass();
            String[] arr1 = {};
            JsonInfuraClass JsonInfuraClass_ref = p.GetJSONData("eth_blockNumber",arr1);
            Console.WriteLine(JsonInfuraClass_ref.result);
            String block_number_hex = JsonInfuraClass_ref.result;
            String block_number_hex_p = block_number_hex.Remove(0, 2);
            int block_number_decimal = int.Parse(block_number_hex_p, System.Globalization.NumberStyles.HexNumber);
            Console.WriteLine(block_number_decimal);


            // check balance of contract
            String [] arr2 = { ro.ContractAddress, "latest"};
            JsonInfuraClass JsonInfuraClass_bal = p.GetJSONData("eth_getBalance", arr2);
            Console.WriteLine(JsonInfuraClass_bal.result);
            String balance_hex = JsonInfuraClass_bal.result;
            String balance_hex_p = balance_hex.Remove(0, 2);
            BigInteger wei = BigInteger.Parse(balance_hex_p, NumberStyles.AllowHexSpecifier);
            Console.WriteLine(wei+" wei");
            decimal eth = (decimal)wei/(1000000000000000000);
            Console.WriteLine(eth+" eth");

            string json2 = null;

            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };

            using (StreamReader r = File.OpenText("../../contract_ABI.json"))
            {
                json2 = r.ReadToEnd();

                using (JsonDocument jsonDoc = JsonDocument.Parse(json2,options))
                {
                    //JsonElement speed = jsonDoc.RootElement.GetProperty("speed");
                    //Console.WriteLine(jsonDoc.RootElement);
                    foreach (var jsonElement in jsonDoc.RootElement.EnumerateArray())
                    {
                        var jsonElement_type = jsonElement.GetProperty("type");
                        var jsonElement_inputs = jsonElement.GetProperty("inputs");


                        foreach (var inputsElement in jsonElement_inputs.EnumerateArray())
                        {
                            var input_type = inputsElement.GetProperty("type");
                            var input_name = inputsElement.GetProperty("name");
                            var internalType = inputsElement.GetProperty("internalType");

                            Console.WriteLine($"type: {input_type} -> name: {input_name} -> internalType: {internalType}");

                        }
                        Console.WriteLine(jsonElement);
                        Console.WriteLine(jsonElement_type);
                        Console.WriteLine("-----------------------------------");
                    }
                }


            }

           



        }
    }
}
