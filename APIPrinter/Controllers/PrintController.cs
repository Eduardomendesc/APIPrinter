using Microsoft.AspNetCore.Mvc;
using APIPrinter.Services;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using System.Globalization;
using System.Linq;
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Text;

namespace APIPrinter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PrintController : ControllerBase
    {
        private readonly Global _logger;
        public PrintController(Global logger)
        {
            _logger = logger;
        }


        //{
        //"command": "BASE64_ENCODED_COMMAND",
        //"hostnameOrIp": "192.168.19.11",
        //"port": 9100
        //}

        [ActionName("Print")]
        [HttpPost]
        public async Task<IActionResult> Print([FromBody] PrintReceiptRequest request)
        {
            // Use the values from the request for hostnameOrIp and port
            var hostnameOrIp = request.HostnameOrIp;
            var port = request.Port;

            var printer = new ImmediateNetworkPrinter(new ImmediateNetworkPrinterSettings()
            {
                ConnectionString = $"{hostnameOrIp}:{port}",
                PrinterName = "TestPrinter"
            });

            var response = true;
            var e = new EPSON();

            try
            {
                // Convert from Base64 to byte array
                byte[] commandBytes = Convert.FromBase64String(request.Command);

                // Send the byte array to the printer
                await printer.WriteAsync(commandBytes);

                response = true;
            }
            catch (Exception ex)
            {
                _logger.LogFile(ex.Message, (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber(), "Utils", "");
                response = false;
            }

            if (response)
            {
                // Return JSON response for success
                return Ok(new { success = true, message = "Impresso com sucesso" });
            }
            else
            {
                // Return JSON response for failure
                return StatusCode(500, new { success = false, message = "Falha na impressão" });
            }
        }

        public class PrintReceiptRequest
        {
            public string Command { get; set; } // Base64 encoded print command
            public string HostnameOrIp { get; set; } // IP address of the printer
            public int Port { get; set; } // Port of the printer
        }
    }
}
