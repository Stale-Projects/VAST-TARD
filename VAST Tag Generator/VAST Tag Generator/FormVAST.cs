using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Globalization;

namespace VAST_Tag_Generator
{
    
    public partial class FormVAST : Form
    {
        //Constantes
        const int WEBM_DEFAULT_WIDTH = 640;
        const int WEBM_DEFAULT_HEIGHT = 360;
        const int WEBM_DEFAULT_BITRATE = 663;
        const string WEBM_TYPE = "video/webm";
        const int MP4_DEFAULT_WIDTH = 640;
        const int MP4_DEFAULT_HEIGHT = 480;
        const int MP4_DEFAULT_BITRATE = 512;
        const string MP4_TYPE = "video/mp4";
        const long SEC_VALUE_IN_TICKS = 10000000; //Un segundo en ticks 
        const int DURACION_DEFAULT_LINEAR_CREATIVE = 30;

        public FormVAST()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //Generar Elementos
                VASTADInLineCreativeLinearMediaFile[] media_files = GenerarMediaFileArray();
                VideoClicks_type video_clicks;
                if (chkIncluirClickthrough.Checked==true || chkIncluirClickTracker.Checked==true)
                {
                    video_clicks = GenerarVideoClicks_Type();
                }
                else
                {
                    video_clicks = null;
                }
                var linear_ad_item_creative_linear = GenerarCreativeLinear(video_clicks, media_files);
                var linear_ad_item_creative_array = GenerarAdInlineCreativeArray(linear_ad_item_creative_linear);
                var impression_tracker_array = GenerarImpressionTrackerArray();
                var inline_ad_item = GenerarInlineAd(linear_ad_item_creative_array, impression_tracker_array);
                var vast_ad_array = Generar_VAST_AD_array(inline_ad_item);

                var vast_tag = new VAST
                {
                    Ad = vast_ad_array,
                    version = "2.0"
                };

                //Write 
                string time_stamp = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() +
                    DateTime.Now.Day.ToString() + "-" + DateTime.Now.Hour.ToString() +
                    DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                var serializer = new XmlSerializer(typeof(VAST));
                string path = @"C:\VASTARD\";
                if (!Directory.Exists(path))
                {
                    //Crear Directorio
                    Directory.CreateDirectory(path);
                }

                using (var stream = new StreamWriter(path + time_stamp + ".xml"))
                    serializer.Serialize(stream, vast_tag);
                MessageBox.Show("Listo!");
            }
            catch (Exception exc)
            {
                //Mostrar mensaje
                MessageBox.Show(exc.Message);
            }
        }

        
        
        private VASTADInLineCreativeLinearMediaFile[] GenerarMediaFileArray()
        {
            //Revision Inicial
            int mediafiles = ((chkIncluir_mp4.Checked) ? 1 : 0) + ((chkIncluir_webm.Checked) ? 1 : 0);
            if (mediafiles == 0) throw new IndexOutOfRangeException("Se debe incluir al menos un Media File");
            if (!System.Uri.IsWellFormedUriString(txtURI_webm.Text, UriKind.Absolute) && chkIncluir_webm.Checked == true)
            {
                throw new InvalidDataException("El URI proporcionado para el webm no es valido");
            }
            if (!System.Uri.IsWellFormedUriString(txtURI_mp4.Text, UriKind.Absolute) && chkIncluir_mp4.Checked == true)
            {
                throw new InvalidDataException("El URI proporcionado para el mp4 no es valido");
            }

            VASTADInLineCreativeLinearMediaFile[] media_files = new VASTADInLineCreativeLinearMediaFile[mediafiles];
            int i = 0;
            int creativeWidth;
            int creativeHeight;
            int creativeBitrate;

            if (chkIncluir_webm.Checked == true)
            {
                int.TryParse(txtWidth_webm.Text, out creativeWidth);
                int.TryParse(txtHeight_webm.Text, out creativeHeight);
                int.TryParse(txtBitrate_webm.Text, out creativeBitrate);
                media_files[i] = new VASTADInLineCreativeLinearMediaFile

                {
                    delivery = VASTADInLineCreativeLinearMediaFileDelivery.progressive,
                    width = ((creativeWidth > 0) ? creativeWidth.ToString() : WEBM_DEFAULT_WIDTH.ToString()),
                    height = ((creativeHeight > 0) ? creativeHeight.ToString() : WEBM_DEFAULT_HEIGHT.ToString()),
                    type = WEBM_TYPE,
                    bitrate = ((creativeBitrate > 0) ? creativeBitrate.ToString() : WEBM_DEFAULT_BITRATE.ToString()),
                    scalable = false,
                    maintainAspectRatio = false,
                    Value = txtURI_webm.Text

                };
                i++;
            }

            if (chkIncluir_mp4.Checked == true)
            {
                int.TryParse(txtWidth_mp4.Text, out creativeWidth);
                int.TryParse(txtHeight_mp4.Text, out creativeHeight);
                int.TryParse(txtBitrate_mp4.Text, out creativeBitrate);
                media_files[i] = new VASTADInLineCreativeLinearMediaFile

                {
                    delivery = VASTADInLineCreativeLinearMediaFileDelivery.progressive,
                    width = ((creativeWidth > 0) ? creativeWidth.ToString() : MP4_DEFAULT_WIDTH.ToString()),
                    height = ((creativeHeight > 0) ? creativeHeight.ToString() : MP4_DEFAULT_HEIGHT.ToString()),
                    type = MP4_TYPE,
                    bitrate = ((creativeBitrate > 0) ? creativeBitrate.ToString() : MP4_DEFAULT_BITRATE.ToString()),
                    scalable = false,
                    maintainAspectRatio = false,
                    Value = txtURI_mp4.Text

                };
            }

            return media_files;

        }

        private VideoClicks_type GenerarVideoClicks_Type()
        {
            //Revision Inicial
            if (!System.Uri.IsWellFormedUriString(txtClickthroughValue.Text, UriKind.Absolute) && chkIncluirClickthrough.Checked == true)
            {
                throw new InvalidDataException("El URI proporcionado para el ClickThrough no es valido");
            }
            if (!System.Uri.IsWellFormedUriString(txtClickTrackerValue.Text, UriKind.Absolute) && chkIncluirClickTracker.Checked == true)
            {
                throw new InvalidDataException("El URI proporcionado para el Click Tracker no es valido");
            }

            var video_click_through = new VideoClicks_typeClickThrough
            {
                id = txtClickthroughID.Text,
                Value = txtClickthroughValue.Text
            };

            //Click Tracking -  TODO: Que se pueda poner más de uno
            VideoClicks_typeClickTracking[] video_click_trackings = new VideoClicks_typeClickTracking[]
            {
                new VideoClicks_typeClickTracking
                {
                    id = txtClickTrackerID.Text,
                    Value = txtClickTrackerValue.Text
                }
            };

            VideoClicks_type video_clicks = new VideoClicks_type
            {
                ClickThrough = video_click_through,
                ClickTracking = video_click_trackings

            };

            return video_clicks;
        }

        private TrackingEvents_typeTracking[] GenerarTrackingEvents()
        {
            //TODO: Agregar controles para poder cargar estos trackers
            //y luego agregarlos al array

            ////Start Event
            //var tracking_event_start = new TrackingEvents_typeTracking
            //{
            //    @event = TrackingEvents_typeTrackingEvent.start,
            //    Value = ""
            //};

            ////First Quartile Event
            //var tracking_event_firstQuartile = new TrackingEvents_typeTracking
            //{
            //    @event = TrackingEvents_typeTrackingEvent.firstQuartile,
            //    Value = ""
            //};

            ////Midpoint Event
            //var tracking_event_midPoint = new TrackingEvents_typeTracking
            //{
            //    @event = TrackingEvents_typeTrackingEvent.midpoint,
            //    Value = ""
            //};

            ////Third Quartile Event
            //var tracking_event_thirdQuartile = new TrackingEvents_typeTracking
            //{
            //    @event = TrackingEvents_typeTrackingEvent.thirdQuartile,
            //    Value = ""
            //};

            ////Complete Event
            //var tracking_event_complete = new TrackingEvents_typeTracking
            //{
            //    @event = TrackingEvents_typeTrackingEvent.complete,
            //    Value = new XCData("www.tracking.com").ToString()

            //};

            
            TrackingEvents_typeTracking[] trackingEvents = new TrackingEvents_typeTracking[]
            {
                //tracking_event_start,
                //tracking_event_firstQuartile,
                //tracking_event_midPoint,
                //tracking_event_thirdQuartile,
                //tracking_event_complete,

            };

            return trackingEvents;
        }

        private VASTADInLineCreativeLinear GenerarCreativeLinear(VideoClicks_type video_clicks, 
            VASTADInLineCreativeLinearMediaFile[] media_files)
        {
            //Calculo de duración
            int segundos_input = 0;
            int segundos = 0;
            int minutos = 0;
            int.TryParse(txtDurationLinear.Text, out segundos_input);
            if (segundos == 0)
            {
                segundos = DURACION_DEFAULT_LINEAR_CREATIVE;
            }

            ConvertirSegundosAMinutosSegundos(segundos_input, out minutos, out segundos);

            var linear_ad_item_creative_linear = new VASTADInLineCreativeLinear
            {
                //Duration = new DateTime(segundos * SEC_VALUE_IN_TICKS, DateTimeKind.Local),
                //TODO: Agregar la siguiente linea cuando agregue TrackingEvents
                //TrackingEvents = trackingEvents,
                //Duration = DateTime.Parse("00:00:" + segundos.ToString()),
                Duration = "00:" + minutos.ToString("00", CultureInfo.CurrentCulture) + ":" + 
                segundos.ToString("00", CultureInfo.CurrentCulture),
                VideoClicks = video_clicks,
                MediaFiles = media_files
            };
            return linear_ad_item_creative_linear;
        }

        

        private VASTADInLineCreative[] GenerarAdInlineCreativeArray(VASTADInLineCreativeLinear linear_ad_item_creative_linear)
        {
            //TODO: Poder generar más de uno - Cambiar el sequence
            var linear_ad_item_creative = new VASTADInLineCreative
            {
                AdID = txtCreativeAdID.Text,
                id = txtCreativeID.Text,
                sequence = "0",
                Item = linear_ad_item_creative_linear
            };

            VASTADInLineCreative[] linear_ad_item_creative_array = new VASTADInLineCreative[]
            {
                linear_ad_item_creative
            };


            return linear_ad_item_creative_array;
        }

        private Impression_type[] GenerarImpressionTrackerArray()
        {
            //TODO: Poder poner más de uno
            //Revisión Inicial
            if (chkIncluirImpressionTracker.Checked==false)
            {
                return null;
            }
            if (!System.Uri.IsWellFormedUriString(txtImpressionTrackerURI.Text, UriKind.Absolute) )
            {
                throw new InvalidDataException("El URI proporcionado para el Impression Tracker no es valido");
            }

            var impression_tracker = new Impression_type
            {
                id = txtImpressionTrackerID.Text,
                Value = txtImpressionTrackerURI.Text
            };

            Impression_type[] impression_tracker_array = new Impression_type[]
            {
                impression_tracker
            };

            return impression_tracker_array;
        }

        private VASTADInLine GenerarInlineAd(VASTADInLineCreative[] linear_ad_item_creative_array,
            Impression_type[] impression_tracker_array)
        {
            var inline_ad = new VASTADInLine
            {
                AdTitle = txtInlineAdTitle.Text,
                Description = txtInlineAdDescription.Text,
                Creatives = linear_ad_item_creative_array,
                Impression = impression_tracker_array
                
            };

            return inline_ad;
        }

        private VASTAD[] Generar_VAST_AD_array(VASTADInLine inline_ad_item)
        {
            var inline_ad = new VASTAD
            {
                id = txtADId.Text,
                Item = inline_ad_item
            };

            VASTAD[] vast_ad_array = new VASTAD[]
            {
                inline_ad
            };
            return vast_ad_array;
        }

        private void ConvertirSegundosAMinutosSegundos(int segundos_in, out int minutos_out, out int segundos_out)
        {
            double resultado = segundos_in / 60;
            segundos_out = (segundos_in > 60) ? segundos_in % 60 : segundos_in;
            minutos_out = (segundos_in > 60) ? (int)Math.Floor(resultado) : 0;
            if (minutos_out > 59)
            {
                minutos_out = 59;
                segundos_out = 59;
            }
        }

        private void txtADId_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
