using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;



public class JsonBinIo
{
    public enum JsonBinIoState
    {
        INITIALIZING = 0,
        READY,
        NUM_OF_STATES
    }

    public enum JsonBinIoTaskState
    {
        NOT_STARTED = 0,
        STARTED,
        FAILED_TO_START,
        COMPLETE,
        ERROR,
        NUM_OF_STATES
    }

    public class JsonBinIoTask
    {
        private JsonBinIoTaskState state;
        private string errorMessage;
        private Task task;
        private string taskCommand;
        private Dictionary<string, string> taskHeaders;
        private HttpMethod method;
        private string result;

        public JsonBinIoTaskState State { get => state; }
        public string ErrorMessage { get => errorMessage;  }
        public Task Task { get => task; set => task = value; }
        public string TaskCommand { get => taskCommand;  }
        public Dictionary<string, string> TaskHeaders { get => taskHeaders;  }
        public HttpMethod Method { get => method;  }
        public string Result { get => result;  }
        

        public JsonBinIoTask(string taskCommand, Dictionary<string, string> taskHeaders, HttpMethod method)
        {
            ResetTask();
            this.taskCommand = taskCommand ?? throw new ArgumentNullException(nameof(taskCommand));
            this.taskHeaders = taskHeaders ?? throw new ArgumentNullException(nameof(taskHeaders));
            this.method = method ?? throw new ArgumentNullException(nameof(method));

            
        }

        public void ResetTask()
        {
            this.state = JsonBinIoTaskState.NOT_STARTED;
            this.errorMessage = "";
            this.task = null;
            this.result = "";
        }

        public async Task SendCommand(HttpClient client, string content = "")
        {
            this.state = JsonBinIoTaskState.STARTED;
            this.result = "";
            this.errorMessage = "";
            
            HttpRequestMessage request = new HttpRequestMessage(method, taskCommand);
            //this seems to cause a bug when moving to 2nd item in task header!
            //foreach (var header in taskHeaders)
            //foreach (KeyValuePair<string, string> header in taskHeaders)
            //{
            //    request.Headers.Add(header.Key, header.Value);
            //}

            

            if((method == HttpMethod.Put) || (method == HttpMethod.Post))
            {
                //also sets the Content-Type header
                request.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json" );
            }
            try
            {
                client.Timeout = TimeSpan.FromSeconds(20);
                using var response = await client.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    state = JsonBinIoTaskState.ERROR;
                    errorMessage = await response.Content.ReadAsStringAsync();
                    return;
                }
                result = await response.Content.ReadAsStringAsync();
                this.state = JsonBinIoTaskState.COMPLETE;
            }
            catch (Exception ex)
            {
                this.state = JsonBinIoTaskState.ERROR;
            }
            
            
            
            
            
        }

        public void FailedToStart(string error)
        {
            errorMessage = error;
            state = JsonBinIoTaskState.FAILED_TO_START;
        }
    }

    private static string xMasterKey = "$2b$10$OQQRgkYW4cM/BqzBesPpGeCNd2ocoS9KdysRxGuS8ZpyrYVKlOgl.";

    static readonly HttpClient client = new HttpClient();
    
    private string binId = "/";

    private JsonBinIoTask createBin;
    private JsonBinIoTask updateBin;
    private JsonBinIoTask readBin;

    private Dictionary<string, string> createBinHeaders = new Dictionary<string, string>();
    private Dictionary<string, string> updateBinHeaders = new Dictionary<string, string>();
    private Dictionary<string, string> readBinHeaders = new Dictionary<string, string>();

    private JsonBinIoState state = JsonBinIoState.INITIALIZING;

    public JsonBinIoState State { get => state; }
    public JsonBinIoTask ReadBin { get => readBin;  }
    public JsonBinIoTask UpdateBin { get => updateBin; }
    public JsonBinIoTask CreateBin { get => createBin; }

    public JsonBinIo(string binId)
    {
        state = JsonBinIoState.INITIALIZING;
        this.binId += binId;

        //create the tasks
        createBinHeaders.Add("Content-Type", "application/json");
        createBinHeaders.Add("X-Master-Key", xMasterKey);
        createBin = new JsonBinIoTask("https://api.jsonbin.io/v3/b", createBinHeaders, HttpMethod.Post);

        updateBinHeaders.Add("X-Master-Key", xMasterKey);
        updateBinHeaders.Add("Content-Type", "application/json");
        
        updateBin = new JsonBinIoTask("https://api.jsonbin.io/v3/b" + this.binId, updateBinHeaders, HttpMethod.Put);

        readBinHeaders.Add("X-Master-Key", xMasterKey);
        readBin = new JsonBinIoTask("https://api.jsonbin.io/v3/b" + this.binId + "?meta=false", readBinHeaders, HttpMethod.Get);

        client.DefaultRequestHeaders.Add("X-Master-Key", xMasterKey);

        state = JsonBinIoState.READY;
        //t = Task.Run(() => CheckBinExists(this.binId));
    }

    //private async Task CheckBinExists(string binId)
    //{
    //    string command = jsonBinReadBinCommand + binId;
    //    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, command);
    //    request.Headers.Add("X-Master-Key", xMasterKey);

    //    var response = await client.SendAsync(request);
    //    if (response.StatusCode == HttpStatusCode.OK)
    //    {
    //        string content = await response.Content.ReadAsStringAsync();
    //    }
                
    //}

    

    public void CreateJsonOnCloud(string json)
    {
        if (binId == "/")
        {
            createBin.FailedToStart("Bin ID Invalid");
            return;
        }
        if (createBin.State == JsonBinIoTaskState.STARTED)
        {

            createBin.FailedToStart("updateBin is still running");
            return;

        }
        createBin.ResetTask();
        createBin.Task = Task.Run(() => createBin.SendCommand(client, json));
    }

    public void GetJsonFromCloud()
    {
        if (binId == "/")
        {
            readBin.FailedToStart("Bin ID Invalid");
            return;
        }
        if (readBin.State == JsonBinIoTaskState.STARTED)
        {

            readBin.FailedToStart("updateBin is still running");
            return;

        }
        readBin.ResetTask();
        readBin.Task = Task.Run(() => readBin.SendCommand(client));
    }

    public void SaveJsonToCloud(string json)
    {
        if (binId == "/")
        {
            updateBin.FailedToStart("Bin ID Invalid");
            return;
        }
        if(updateBin.State == JsonBinIoTaskState.STARTED)
        {
            
            updateBin.FailedToStart("updateBin is still running");
            return;

        }
        updateBin.ResetTask();
        updateBin.Task = Task.Run(() => updateBin.SendCommand(client, json));
    }
}
