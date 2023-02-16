using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;


namespace DBTables
{
    [Serializable]
    public class MessageLocation
    {
        public string UserID;
        public string Text;
        public float Latitude;
        public float Longitude;
        
        [JsonIgnore]
        public DateTime DateCreated = DateTime.Now;

        [JsonProperty("DateCreated")]
        private long DateCreatedTicks
        {
            get { return (int)(this.DateCreated - DateTime.UnixEpoch).TotalSeconds; }
            set { this.DateCreated = DateTime.UnixEpoch.AddSeconds(Convert.ToInt32(value)); }
        }


        public MessageLocation() { }

        public MessageLocation(string user, string text, LocationInfo locationInfo, DateTime? date = null)
        {
            UserID = user;
            Text = text;
            Latitude = locationInfo.latitude;
            Longitude = locationInfo.longitude;
            DateCreated = date != null ? (DateTime)date : DateCreated;
        }

        public MessageLocation(string user, string text, float latitude, float longitude, DateTime? date = null)
        {
            UserID = user;
            Text = text;
            Latitude = latitude;
            Longitude = longitude;
            DateCreated = date != null ? (DateTime)date : DateCreated;
        }
    }
}
