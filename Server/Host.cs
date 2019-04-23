using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Server.Interfaces;
using Server.Attributes;
using Newtonsoft.Json.Linq;

namespace Server
{
    public class Host
    {
        private HttpListener Listener;
        private string prefix = "http://localhost:8888/";
        private bool IsRunning = false;
        private IControllerCreator controllerFactory;

        public Host(IControllerCreator factory)
        {
            controllerFactory = factory;
        }

        public void Start()
        {
            if (!IsRunning)
            {
                Listener = new HttpListener();
                Listener.Prefixes.Clear();
                Listener.Prefixes.Add(prefix);
                Listener.Start();
                IsRunning = true;
                Task.Factory.StartNew(async () => await Listen());
            }
        }

        private async Task Listen()
        {
            while (IsRunning)
            {
                try
                {
                    Console.WriteLine("It's listenning...");
                    var context = await Listener.GetContextAsync();
                    Console.WriteLine("Got Context");
                    await Task.Factory.StartNew(async () => await HandleContextAsync(context));
                }
                catch (HttpListenerException)
                {
                    Console.WriteLine("It's stopped");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n{e.GetType().Name}\n{e.Message}\n");
                }
            }
        }
        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                Listener.Stop();
            }
        }

        private async Task HandleContextAsync(HttpListenerContext context)
        {

            var request = context.Request;
            var response = context.Response;

            string controllerName = request.Url.Segments[1].Replace("/", "").ToLower();
            Controller controller = controllerFactory.Create(controllerName);

            if (controller == null)
            {
                Console.WriteLine("controller is not found");
                response.StatusCode = (int)HttpStatusCode.NotFound;
                using (Stream stream = response.OutputStream) { }
                return;
            }

            string methodName = request.Url.Segments[2].Replace("/", "");
            string[] namesOfMethodParams = request.Url
                                        .Segments
                                        .Skip(3)
                                        .Select(s => s.Replace("/", ""))
                                        .ToArray();

            var methods = controller
                       .GetType()
                       .GetMethods()
                       .Where(m => m.Name.ToLower() == methodName.ToLower())
                       .Where(m => m.GetCustomAttributes(typeof(Http), true)
                           .Any(attr =>
                                    ((Http)attr).Type.ToLower() == request.HttpMethod.ToLower())
                                        || (m.GetCustomAttributes(typeof(Http), true).FirstOrDefault() == null
                                            && request.HttpMethod.ToLower() == "get"
                                            )
                                )
                      .Where(m => m.GetParameters().Length == namesOfMethodParams.Length
                            || (m.GetParameters().
                                    Any(p => p.GetCustomAttributes(typeof(FromBody), true).FirstOrDefault() != null) // а лучше?
                                && m.GetParameters().Length - 1 == namesOfMethodParams.Length)
                            );

            var method = methods.FirstOrDefault();

            if (method == null)
            {
                Console.WriteLine("method is not found");
                response.StatusCode = (int)HttpStatusCode.NotFound;
                using (Stream stream = response.OutputStream) { }
                return;
            }

            //-----------------------------------!!КОСТЫЛЬ!!-----------------------------------//

            var methodParamsInfo = method.GetParameters();
            object[] methodParamsValues = new object[methodParamsInfo.Length];//??
            int k = 0;
            for (int i = 0; i < methodParamsInfo.Length; i++)
            {

                if (methodParamsInfo[i].GetCustomAttributes(typeof(FromBody), true).FirstOrDefault() != null)
                {
                    k++;
                    if (request.HasEntityBody && method.GetCustomAttributes(typeof(Http), true)
                            .Any(attr => ((Http)attr).Type.ToLower() == "post"
                                || ((Http)attr).Type.ToLower() == "put")
                       )
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                            methodParamsValues[i] = JsonConvert.DeserializeObject(reader.ReadToEnd(), methodParamsInfo[i].ParameterType);
                    else
                        methodParamsValues[i] = null;
                }
                else
                    methodParamsValues[i] = Convert.ChangeType(namesOfMethodParams[i - k], methodParamsInfo[i].ParameterType);
            }

            //-----------------------------------!!КОСТЫЛЬ!!-----------------------------------//
            if (method.ReturnType.Name == "Void")
            {
                method.Invoke(controller, methodParamsValues);
                response.StatusCode = (int)HttpStatusCode.OK;
                using (Stream strema = response.OutputStream) { }
            }
            else
            {
                object answer = method.Invoke(controller, methodParamsValues);
                if(answer == null)
                {
                    Console.WriteLine("answer is empty");
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    using (Stream stream = response.OutputStream) { }
                    return;
                }
                var jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                string answerJson;
                Console.WriteLine("Begin serialization");
                try
                {
                    answerJson = JsonConvert.SerializeObject(answer, jsonSerializerSettings);
                }
                catch(Exception e)
                {
                    answerJson = "";
                    Console.WriteLine($"{e.Source}\n{e.Message}");
                }
                Console.WriteLine("End serialization");
                byte[] buffer = Encoding.UTF8.GetBytes(answerJson);
                response.ContentType = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = buffer.Length;
                response.StatusCode = (int)HttpStatusCode.OK;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            Console.WriteLine();

        }
    }
}
