using Microsoft.AspNetCore.Mvc;
using Post.Common.DTOs;
using System.Text;

namespace Post.Cmd.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class Base64DecodeController : ControllerBase
    {
        [HttpPost]
        public IActionResult DecodeEvents([FromBody] RootObject[] eventStoreCollection, [FromQuery] bool flatten = false, [FromQuery] bool orderLogicalSequence = false)
        {
            try
            {
                if (orderLogicalSequence)
                {
                    eventStoreCollection = eventStoreCollection.OrderBy(x => x.AggregateIdentifierDecoded).ThenBy(x => x.TimeStamp).ToArray();
                }

                if (flatten)
                {
                    int? maxAuthorCharacters = eventStoreCollection.Max(x => x.EventData.Author?.Length);
                    int maxEventTypeCharacters = eventStoreCollection.Max(x => x.EventType.Length);
                    StringBuilder sb = new StringBuilder();
                    foreach (var e in eventStoreCollection)
                        sb.AppendLine(e.ToString(maxAuthorCharacters, maxEventTypeCharacters));

                    return Ok(sb.ToString());
                }

                return Ok(eventStoreCollection);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse { Message = ex.Message });
            }
        }

        [HttpPost("decodeArray")]
        public IActionResult Decode([FromBody] string[] base64)
        {
            try
            {
                List<string> decodedStrings = new List<string>();

                foreach (string s in base64)
                {
                    byte[] decodedBytes = Convert.FromBase64String(s);
                    string decodedString = BitConverter.ToString(decodedBytes);

                    Guid decodedGuid;
                    bool isDecoded = Guid.TryParse(decodedString.Replace("-", ""), out decodedGuid);

                    if (isDecoded)
                    {
                        decodedString = decodedGuid.ToString();
                    }

                    decodedStrings.Add(decodedString);
                }

                // Print the decoded string
                return Ok(decodedStrings);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse { Message = ex.Message });
            }

        }
    }

    #region Demo models
    public class EventData
    {
        public string _t { get; set; }
        public string _id { get; set; }
        public Guid IdDecoded { get { return RootObject.DecodeBase64(_id); } }
        public int Version { get; set; }
        public string Type { get; set; }
        public string Author { get; set; }
        public string Message { get; set; }
        public DateTime DatePosted { get; set; }
    }

    public class RootObject
    {
        public string _id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string AggregateIdentifier { get; set; }
        public Guid AggregateIdentifierDecoded { get { return DecodeBase64(AggregateIdentifier); } }
        public string AggregateType { get; set; }
        public int Version { get; set; }
        public string EventType { get; set; }
        public EventData EventData { get; set; }

        public string ToString(int? maxAuthorCharacters, int maxEventTypeCharacters)
        {
            string version = Version < 10 ? Version.ToString("' '0") : Version.ToString();
            // Fill empty spaces in each string up to the length of the longest string
            string eventType = EventType.PadRight(maxEventTypeCharacters);

            EventData.Author = EventData.Author ?? string.Empty;
            string author = EventData.Author?.PadRight(maxAuthorCharacters ?? 0);
            return $"AggId: {AggregateIdentifierDecoded} \tCreated: {TimeStamp.ToString("yyyy-MM-dd hh:mm:ss")} \tVersion: {version} \t\tType: {eventType} \t\tAuthor: {author} \t\tMessage: {EventData.Message}";
        }

        internal static Guid DecodeBase64(string stringGuid)
        {
            byte[] decodedBytes = Convert.FromBase64String(stringGuid);
            string decodedString = BitConverter.ToString(decodedBytes);

            Guid decodedGuid;
            Guid.TryParse(decodedString.Replace("-", ""), out decodedGuid);

            return decodedGuid;
        }
    }
    #endregion
}
