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
    /// <summary>
    /// Класс сервера, реализующего прослушку входящих http запросов
    /// </summary>
    public class Host
    {
        private HttpListener Listener;
        private string prefix = "http://localhost:8888/";
        private bool IsRunning = false;
        // Фабрика для создания контроллеров
        private IControllerCreator controllerFactory;

        public Host(IControllerCreator factory)
        {
            controllerFactory = factory;
        }

        /// <summary>
        /// Запуск сервера
        /// </summary>
        public async void Start()
        {
            if (!IsRunning)
            {
                Listener = new HttpListener();
                Listener.Prefixes.Clear();
                Listener.Prefixes.Add(prefix);
                Listener.Start();
                IsRunning = true;
                //Task.Factory.StartNew(async () => await Listen());
                await Listen();
            }
        }

        /// <summary>
        /// Прослушка входящих запросов
        /// </summary>
        /// <returns></returns>
        private async Task Listen()
        {
            while (IsRunning)
            {
                try
                {
                    Console.WriteLine("It's listenning...");
                    var context = await Listener.GetContextAsync();
                    Console.WriteLine("Got Context");
                    //await Task.Factory.StartNew(async () => await HandleContextAsync(context));
                    await Task.Factory.StartNew(() => HandleContext(context));
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

        /// <summary>
        /// Остановка сервера
        /// </summary>
        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                Listener.Stop();
            }
        }

        /// <summary>
        /// Обработка запроса
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private void HandleContext(HttpListenerContext context)
        //private async Task HandleContextAsync(HttpListenerContext context)
        {

            var request = context.Request;
            var response = context.Response;

            // Получаем имя контроллера из запроса
            string controllerName = request.Url.Segments[1].Replace("/", "").ToLower();
            // Создаем контроллев используя фабрику
            Controller controller = controllerFactory.Create(controllerName);

            if (controller == null)
            {
                Console.WriteLine("controller is not found");
                response.StatusCode = (int)HttpStatusCode.NotFound;
                using (Stream stream = response.OutputStream) { }
                return;
            }
            
            // Получаем имя метода из запроса
            string methodName = request.Url.Segments[2].Replace("/", "");
            // Получаем строковые значение параметров запроса
            string[] strMethodParams = request.Url
                                        .Segments
                                        .Skip(3)
                                        .Select(s => s.Replace("/", ""))
                                        .ToArray();

            // Ищем методы контроллера
            var methods = controller
                       .GetType()
                       .GetMethods()
                       // поиск по имени
                       .Where(m => m.Name.ToLower() == methodName.ToLower())
                       // по соответствию типу запроса
                       .Where(m => m.GetCustomAttributes(typeof(Http), true)
                           .Any(attr =>
                                    ((Http)attr).Type.ToLower() == request.HttpMethod.ToLower())
                                        || (m.GetCustomAttributes(typeof(Http), true).FirstOrDefault() == null
                                            && request.HttpMethod.ToLower() == "get"
                                            )
                                )
                        // по количеству параметров
                        .Where(m => m.GetParameters().Length == strMethodParams.Length
                            || (strMethodParams.Length ==
                                    m.GetParameters().Length
                                    - m.GetParameters().Where(mp => mp.HasDefaultValue).Count()
                                    - (m.GetParameters()
                                        .Any(p => p.GetCustomAttributes(typeof(FromBody), true)
                                            .FirstOrDefault() != null) ? 1 : 0
                                    )
                                )
                            );

            // Берем первый метода из найденных
            var method = methods.FirstOrDefault();

            if (method == null)
            {
                Console.WriteLine("method is not found");
                response.StatusCode = (int)HttpStatusCode.NotFound;
                using (Stream stream = response.OutputStream) { }
                return;
            }

            // Получеам информацию о параметрах найденного метода
            var methodParamsInfo = method.GetParameters();
            // Массив параметров, используемый для вызова найденного параметра
            object[] methodParamsValues = new object[methodParamsInfo.Length];

            // Счетчик, нужен для пропуска параметров метода с аттрибутом FromBody
            int k = 0;
            // Определение настроек сериализации
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;
            jsonSerializerSettings.CheckAdditionalContent = true;   
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Error;

            // Определение параметров methodParamValues метода
            for (int i = 0; i < methodParamsInfo.Length; i++)
            {
                // Проверка на значение параметра по умолчанию
                if (methodParamsInfo[i].HasDefaultValue)
                {
                    methodParamsValues[i] = methodParamsInfo[i].DefaultValue;
                    continue;
                }

                // Если параметр имеет аттрибут FromBody
                if (methodParamsInfo[i].GetCustomAttributes(typeof(FromBody), true).FirstOrDefault() != null)
                {
                    k++;
                    // Если параметр запрос иметт тело и метод имеет аттрибут Http c типом запроса POST или PUT
                    if (request.HasEntityBody && method.GetCustomAttributes(typeof(Http), true)
                            .Any(attr => ((Http)attr).Type.ToLower() == "post"
                                || ((Http)attr).Type.ToLower() == "put")
                       )
                        try
                        {
                            // Десериализация json-тела запроса
                            
                            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                                methodParamsValues[i] = JsonConvert.DeserializeObject(reader.ReadToEnd(), methodParamsInfo[i].ParameterType);
                        }
                        catch (Exception exc)
                        {
                            Console.WriteLine("json error");
                            Console.WriteLine($"{exc.Source}\n{exc.Message}");
                        }
                    else
                        methodParamsValues[i] = null;
                }
                // Присваивание значений параматрам метода если они не имеют атрибут FromBody
                else
                    methodParamsValues[i] = Convert.ChangeType(strMethodParams[i - k], methodParamsInfo[i].ParameterType);
                if (methodParamsValues[i] == null)
                    Console.WriteLine("is null");
            }

            // Если метод не имеет возвращаемого значения, то просто вызываем метод
            if (method.ReturnType.Name == "Void")
            {
                method.Invoke(controller, methodParamsValues);
                response.StatusCode = (int)HttpStatusCode.OK;
                using (Stream strema = response.OutputStream) { }
            }
            // если метод имеет возвращаемоге значение, то сериализуем его(значение) и посылаем клиенту
            else
            {
                object answer = method.Invoke(controller, methodParamsValues);
                if (answer == null)
                {
                    Console.WriteLine("answer is empty");
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    using (Stream stream = response.OutputStream) { }
                    return;
                }
               
                string answerJson;
                Console.WriteLine("Begin serialization");
                try
                {
                    answerJson = JsonConvert.SerializeObject(answer, jsonSerializerSettings);
                }
                catch (Exception e)
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
