using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    public class PowerShellController : Controller
    {
        [HttpGet]
        public IActionResult RunScript()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RunScript(string scriptPath)
        {
            if (System.IO.File.Exists(scriptPath) && Path.GetExtension(scriptPath) == ".ps1")
            {
                // Set response headers for SSE
                Response.Headers.Add("Content-Type", "text/event-stream");
                Response.Headers.Add("Cache-Control", "no-cache");
                Response.Headers.Add("Connection", "keep-alive");

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy unrestricted -File \"{scriptPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = psi })
                {
                    process.Start();

                    // Read the output stream asynchronously and stream it to the client
                    using (var reader = process.StandardOutput)
                    {
                        while (!reader.EndOfStream)
                        {
                            string outputLine = await reader.ReadLineAsync();
                            if (outputLine != null)
                            {
                                byte[] data = Encoding.UTF8.GetBytes($"data: {outputLine}\n\n");
                                await Response.Body.WriteAsync(data, 0, data.Length);
                                await Response.Body.FlushAsync();
                            }
                        }
                    }

                    process.WaitForExit();
                }

                return new EmptyResult();
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid PowerShell script path.";
                return View("RunScript", scriptPath);
            }
        }
        private readonly ScriptDbContext _context;

        public PowerShellController(ScriptDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult RunScript()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RunScript(RunScriptViewModel model)
        {
            if (ModelState.IsValid)
            {
                var scriptPath = new ScriptPath { Path = model.ScriptPath };
                _context.ScriptPaths.Add(scriptPath);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home"); // Redirect to home or another page
            }

            return View(model);
        }
    }
}
