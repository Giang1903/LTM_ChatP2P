using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ChatP2P.Model
{
    public class ConversationSerializer
    {
        private string? hostEndpoint = null;
        private string? hostName = null;

        private readonly object _lock = new object();

        // Thư mục gốc lưu các cuộc trò chuyện
        private string baseDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SavedChats");

        public ConversationSerializer() { }

        // Lưu một cuộc trò chuyện
        public void Save(ConversationModel conversationModel)
        {
            InitializeHost();
            // Tạo thư mục dựa trên tên và endpoint của host
            string directoryPath = Path.Combine(baseDirectory, $"{hostName}_{hostEndpoint}");
            Directory.CreateDirectory(directoryPath);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            string filePath = Path.Combine(directoryPath, $"{conversationModel.User.Name}_{conversationModel.User.Ip}-{conversationModel.User.Port}.json");

            string json = JsonConvert.SerializeObject(conversationModel, settings);

            File.WriteAllText(filePath, json);
        }

        // Tải một cuộc trò chuyện từ file JSON
        public ConversationModel Load(string filePath)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            string json = File.ReadAllText(filePath);

            ConversationModel conversation = JsonConvert.DeserializeObject<ConversationModel>(json, settings);

            return conversation;
        }

        // Tải tất cả cuộc trò chuyện từ thư mục lịch sử
        public List<ConversationModel> LoadAll()
        {
            try
            {
                InitializeHost();
                List<ConversationModel> conversationModels = new List<ConversationModel>();

                string directoryPath = Path.Combine(baseDirectory, $"{hostName}_{hostEndpoint}");

                if (Directory.Exists(directoryPath))
                {
                    string[] files = Directory.GetFiles(directoryPath);

                    try
                    {
                        foreach (string file in files)
                        {
                            ConversationModel conversation = Load(file);
                            conversationModels.Add(conversation);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ghi log lỗi nếu đọc file không thành công
                        System.Diagnostics.Debug.WriteLine("Lỗi khi đọc file: " + ex.Message);
                    }
                }

                return conversationModels;
            }
            catch (Exception ex)
            {
                // Trả về danh sách rỗng nếu xảy ra lỗi
                return new List<ConversationModel>();
            }
        }

        // Khởi tạo thông tin host nếu chưa có
        private void InitializeHost()
        {
            if (hostEndpoint == null || hostName == null)
            {
                hostEndpoint = NetworkManager.Instance.Host.Ip + "-" + NetworkManager.Instance.Host.Port;
                hostName = NetworkManager.Instance.Host.Name;
            }
        }
    }
}
