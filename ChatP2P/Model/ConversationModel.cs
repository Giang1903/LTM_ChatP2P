using System;
using System.Collections.ObjectModel;

namespace ChatP2P.Model
{
    public class ConversationModel
    {
        public string ConversationId { get; }
        public UserModel Self { get; }
        public UserModel Peer { get; }
        public bool IsActive { get; set; }
        public ObservableCollection<DataModel> Messages { get; }

        public ConversationModel(UserModel self, UserModel peer, bool isActive = true)
        {
            ConversationId = GenerateConversationId(self, peer);
            Self = self;
            Peer = peer;
            IsActive = isActive;
            Messages = new ObservableCollection<DataModel>();
        }

        public void Append(DataModel data)
        {
            Messages.Add(data);
        }

        private static string GenerateConversationId(UserModel a, UserModel b)
        {
            var p1 = $"{a.Address}:{a.Port}";
            var p2 = $"{b.Address}:{b.Port}";
            return string.CompareOrdinal(p1, p2) <= 0 ? $"{p1}|{p2}" : $"{p2}|{p1}";
        }
    }
}
