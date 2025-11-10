using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ChatP2P.Model
{
    internal class ConversationSerializer
    {
        private readonly string storageDirectory;
        private readonly string storageFilePath;

        public ConversationSerializer()
        {
            storageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChatP2P");
            storageFilePath = Path.Combine(storageDirectory, "conversations.json");
        }

        private class SerializableConversation
        {
            public string Address { get; set; }
            public string Name { get; set; }
            public List<SerializableMessage> Messages { get; set; }
        }

        private class SerializableMessage
        {
            public string SenderAddr { get; set; }
            public string ReceiverAddr { get; set; }
            public string Message { get; set; }
            public DateTime Date { get; set; }
            public string Name { get; set; }
        }

        public void Save(IEnumerable<ConversationModel> conversations)
        {
            if (!Directory.Exists(storageDirectory))
            {
                Directory.CreateDirectory(storageDirectory);
            }

            var data = conversations.Select(conv => new SerializableConversation
            {
                Address = conv.User.Address,
                Name = conv.User.Name,
                Messages = conv.Messages.Select(m => new SerializableMessage
                {
                    SenderAddr = m.SenderAddr,
                    ReceiverAddr = m.ReceiverAddr,
                    Message = m.Message,
                    Date = m.Date,
                    Name = m.Name
                }).ToList()
            }).ToList();

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(storageFilePath, json);
        }

        public IEnumerable<ConversationModel> Load()
        {
            if (!File.Exists(storageFilePath))
            {
                return Enumerable.Empty<ConversationModel>();
            }

            try
            {
                var json = File.ReadAllText(storageFilePath);
                var data = JsonConvert.DeserializeObject<List<SerializableConversation>>(json) ?? new List<SerializableConversation>();

                var restored = new List<ConversationModel>();
                foreach (var sc in data)
                {
                    var parts = sc.Address.Split(':');
                    string ip = parts.Length > 0 ? parts[0] : "";
                    string port = parts.Length > 1 ? parts[1] : "";
                    var user = new UserModel(ip, port, sc.Name);
                    var conv = new ConversationModel(user);

                    foreach (var sm in sc.Messages.OrderBy(m => m.Date))
                    {
                        // Khôi phục theo hướng tin nhắn lưu trữ
                        var senderParts = (sm.SenderAddr ?? "").Split(':');
                        var senderUser = senderParts.Length >= 2 && sm.SenderAddr == user.Address
                            ? user
                            : new UserModel(senderParts.ElementAtOrDefault(0) ?? "", senderParts.ElementAtOrDefault(1) ?? "", sm.Name);

                        var msg = new MessageModel(senderUser, sm.ReceiverAddr, sm.Message);
                        // Không có setter Date trong DataModel, nên LastActivity sẽ dựa vào thời điểm thêm
                        conv.ReceiveMessage(msg);
                    }

                    restored.Add(conv);
                }

                return restored;
            }
            catch
            {
                return Enumerable.Empty<ConversationModel>();
            }
        }
    }
}
