using System;
using System.IO;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.SecurityModel;

namespace AutoAltTags
{
    public class SetAutoAltTag
    {
        public string ApiKey { get; set; }

        protected void OnItemSaved(object sender, EventArgs args)
        {
            if (args == null)
                return;
            
            SitecoreEventArgs sitecoreArgs = args as SitecoreEventArgs;
            Assert.IsNotNull(sitecoreArgs, "eventArgs");

            SitecoreEventArgs eventArgs = args as SitecoreEventArgs;
            Assert.IsNotNull(eventArgs, "eventArgs");

            if (eventArgs != null)
            {
                Item item = eventArgs.Parameters[0] as Item;
                if (item == null || !item.Paths.IsMediaItem)
                    return;

                MediaItem mediaItem = item;

                if (string.IsNullOrEmpty(mediaItem.Extension) || !string.IsNullOrEmpty(mediaItem.Alt))
                    return;

                Stream mediaStream = mediaItem.GetMediaStream();
                if (mediaStream == null || mediaStream.Length == 0)
                    return;


                if (!string.IsNullOrEmpty(ApiKey))
                {
                    Log.Error("SetAutoAltTag: No API Key Specified - Module disabled", GetType());
                    return;
                }

                var describeImage = new DescribeImage(ApiKey);

                using (new SecurityDisabler())
                {
                    using (new EditContext(mediaItem))
                    {
                        try
                        {
                            mediaItem.Alt = describeImage.GetDescription(mediaStream);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("SetAutoAltTag: Excetion occured " + ex.Message, ex, GetType());
                        }
                    }
                }
            }
        }
    }
}
