using Confluent.Kafka;
using Newtonsoft.Json;
using Serilog.Core;
using Serilog.Events;

namespace Core.Utilities.Assistant;

public class KafkaSink : ILogEventSink
  {
    private readonly IProducer<string, string> kafkaProducer;
    private readonly string _kafkaTopic;
    private readonly string _projectTitle;
    private readonly string _environment;
    private readonly string _ip;

    public KafkaSink(
      string bootstrapServers,
      string topic,
      string projectTitle,
      string environment,
      string ip)
    {
      this._kafkaTopic = topic;
      this._projectTitle = projectTitle;
      this._environment = environment;
      this._ip = ip;
      ProducerConfig config = new ProducerConfig();
      config.BootstrapServers = bootstrapServers;
      this.kafkaProducer = new ProducerBuilder<string, string>((IEnumerable<KeyValuePair<string, string>>) config).Build();
    }

    public void Emit(LogEvent logEvent)
    {
      LogEventPropertyValue eventPropertyValue1;
      LogEventPropertyValue eventPropertyValue2 = logEvent.Properties.TryGetValue("MessageFa", out eventPropertyValue1) ? eventPropertyValue1 : (LogEventPropertyValue) null;
      LogEventPropertyValue eventPropertyValue3;
      LogEventPropertyValue eventPropertyValue4 = logEvent.Properties.TryGetValue("Path", out eventPropertyValue3) ? eventPropertyValue3 : (LogEventPropertyValue) null;
      LogEventPropertyValue eventPropertyValue5;
      LogEventPropertyValue eventPropertyValue6 = logEvent.Properties.TryGetValue("Username", out eventPropertyValue5) ? eventPropertyValue5 : (LogEventPropertyValue) null;
      LogEventPropertyValue eventPropertyValue7;
      LogEventPropertyValue eventPropertyValue8 = logEvent.Properties.TryGetValue("Workspace", out eventPropertyValue7) ? eventPropertyValue7 : (LogEventPropertyValue) null;
      LogEventPropertyValue eventPropertyValue9;
      LogEventPropertyValue eventPropertyValue10 = logEvent.Properties.TryGetValue("CustomObject", out eventPropertyValue9) ? eventPropertyValue9 : (LogEventPropertyValue) null;
      string str1 = eventPropertyValue2?.ToString().Trim('"');
      string str2 = eventPropertyValue4?.ToString().Trim('"');
      string str3 = eventPropertyValue6?.ToString().Trim('"');
      string str4 = eventPropertyValue8?.ToString().Trim('"');
      string str5 = eventPropertyValue10?.ToString().Trim('"');
      var data = new
      {
        Timestamp = logEvent.Timestamp,
        Level = logEvent.Level.ToString(),
        Message = logEvent.Exception?.Message,
        ProjectTitle = this._projectTitle,
        Environment = this._environment,
        IP = this._ip,
        MessageFa = str1,
        Path = str2,
        Username = str3,
        Workspace = str4,
        CustomObject = str5
      };
      string str6 = JsonConvert.SerializeObject((object) data);
      IProducer<string, string> kafkaProducer = this.kafkaProducer;
      string kafkaTopic = this._kafkaTopic;
      Message<string, string> message = new Message<string, string>();
      message.Key = (string) null;
      message.Value = str6;
      CancellationToken cancellationToken = new CancellationToken();
      kafkaProducer.ProduceAsync(kafkaTopic, message, cancellationToken).Wait();
    }
  }