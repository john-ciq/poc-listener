using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChartIQ.Finsemble;
using Microsoft.IdentityModel.Tokens;
using ChartIQ.Finsemble.Events;
using Newtonsoft.Json.Linq;
using System.Diagnostics;


namespace AppListener
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window
    {

        const string queryResponderChannel = "FIN-2264.responder";
        const string listenerChannel = "FIN-2264.listener";

        private JsonWebKey JWK = new JsonWebKey()
        {
            D = "S7msrBKYM_VhXmAWTLhoRobLTevToYbX3xkbkN-EiaZ6Hg-xfozn5uAQGnBnoP1ldKOgoj5Z3dx6kTgR-3xfonEfdkk6wn0OVNbuFYyGkeeV4ts5JmyVpihFqE3RbkWuQ5D5xpIhXWl1fOWEuFfGCYIib2pmBUyc4Lz4OYmMOIGEC9nJg6ZuoKOh0nDZBjjO6vbYbXCEi0ys-FD7NAWsM8jTNDxLyXmpCNSVJOnGTX9CcxnFGdLVO8fqbooaydSHtFJE9YVqUKWp54hOBFMHdsTY5iT88urrvdBLxtGf6NGUVetpw-nFiOihDRPb9wMuLY9CT4DDzLecxadrLKh0PQ",
            DP = "dw8tnJQEpXazaYFcOhCvlU0Y4kGiul1W-_MTCXml8njCEx0Gp4s8jWf_QK7PcdzZRl-t_NTu12i2UGn8lCvOrSc4g66OkwZxPCVuvGXqQQ2DHTgFR2vk-Q53cFrMtjo8FvplkQuf92vS58ulq-iogDp7xxxXTAhmWaPA_d2i3C0",
            DQ = "EffO_SIA7qpBsSDHDKN3TdgybckYjN738roGKcU23ZXDDRy8h9X_lYtMSvjBQz7CRdia7F7aXGJLS08LSAfajRH-W8ssYfRge3twkrqxDXspWwIb77eMSINUUzRA1QQD7kSs_-LsU-rujsqZa-dnBxWhareFtVM-957lkzp4lTU",
            E = "AQAB",
            N = "zPOxYfLiAd3rM7KOLDBIeLl0kjQ7fk43mTTc1Nm9BDaSNVWqvOshSMCHmqOrKZX_WwA67Y6CQxWI5rZ6WNzoLHQU3PyqOvAdB7RgXCHlSeVYZ2haTcbWRjAXQ89H1WfnW96VwAbZn5nccxQGYlZIl3AMcwNRqV4hmiKtJVq4-2OzA-zs9Yg4Pfs6TbKR1XbNKz6EAPHDev0-7Xdnb6x1-qr4uL2Bq0ZwgfnyKQIRLc3zhD5kwqiJ5N7uZAZomLRkMFMXwy1UftD6fQUlFT2yoISn403RwN7YHEL8KoA9X7Jgs-dtlBh8c38QzQ1vbdMBzPuyRb07GMMKb6bdjdBV1w",
            P = "6RJ2697p-SS6CIJYdDU8AxgyKHx3kMkHnFzLQakoi1h6UUNxb-r8v1PUU5xZA5ijmvJv9Ag5uIOhT3u3vSOkADqGsqctNGtFf60hbjnkMDQnDNr2b-1_ayCn2pQM6mmisASfcnOJKUag4tLw3weK-t5uN9KYqfDBU7fVjqnq8HM",
            Q = "4R0RUlCnmZRzEw9sqjwxMuvNM1BTSubvMvG0VIlBkYbCn9MOdwurBPxrYqnUcbw-q-qzQy6st6a4L-EAZSnfD3FEEFKeINOJK06l0EwjcLeP8B4YQ-bxd9UroXpl9ACiMqHzyvJCNOpw8A22nbjKVnVhW1E17F-LFAJoWBetYA0",
            QI = "PODgpJrXxPAp72v_O0fNfAhWjHLeTk9TfLARl9lzPpYIoYR5tgP1Y_A-3feH_xtCfkzcCskfXIerQlY9lVmqs-eGEYjfuuPVYIruN4OsskMY1nz-h_14clyUmUwfCQJDV4qjcAzf80IMu53jYEW1BydRf90snRjk1dYgSq_qtTQ",
        };
        private Finsemble FSBL;

        public MainWindow()
        {
            this.Closing += MainWindow_Closing;
            FSBL = new Finsemble(App.args, this);
            FSBL.Connected += FSBL_Connected;
            FSBL.Connect("WPF_POC", JWK);
        }

        private EventHandler<ChartIQ.Finsemble.Router.FinsembleQueryArgs> createQueryResponder() {
            return (err, args) =>
            {
                Trace.TraceError("responder recieved message: " + args.response.ToString());
                FSBL.Logger.System.Log(new JToken[] { "responder recieved message", args.response.ToString() });

                // Respond
                args?.sendQueryMessage(new FinsembleEventResponse(new JObject()
                {
                    ["canceled"] = false,
                    ["data"] = args.response
                },
                null));

                // Remove the responder
                FSBL.RouterClient.RemoveResponder(queryResponderChannel);
                Trace.TraceError("responder removed");
                FSBL.Logger.System.Log(new JToken[] { "responder removed" });

                // Add a new responder
                FSBL.RouterClient.AddResponder(queryResponderChannel, createQueryResponder());
                Trace.TraceError("responder added");
                FSBL.Logger.System.Log(new JToken[] { "responder added again" });
            };
        }

        private EventHandler<ChartIQ.Finsemble.Router.FinsembleEventArgs> createListenerHandler()
        {
            EventHandler<ChartIQ.Finsemble.Router.FinsembleEventArgs> listener = (err, args) => {
                FSBL.Logger.System.Log(new JToken[] {"listener recieved message", args.response.ToString()});
            };
            return listener;
        }
        private EventHandler<ChartIQ.Finsemble.Router.FinsembleEventArgs> wrapListenerHandler(string channel, EventHandler<ChartIQ.Finsemble.Router.FinsembleEventArgs> listener)
        {
            EventHandler<ChartIQ.Finsemble.Router.FinsembleEventArgs> wrappedListener = null;
            wrappedListener = (err, args) => {
                listener(err, args);
                FSBL.RouterClient.RemoveListener(channel, wrappedListener);
                Trace.TraceError("listener removed");
                FSBL.Logger.System.Log(new JToken[] { "listener removed" });

                // Add a new listener
                FSBL.RouterClient.AddListener(channel, wrapListenerHandler(channel, listener));
                Trace.TraceError("listener added");
                FSBL.Logger.System.Log(new JToken[] { "listener added" });
            };
            return wrappedListener;
        }

        private void FSBL_Connected(object sender, EventArgs e)
        {
            EventHandler<ChartIQ.Finsemble.Router.FinsembleQueryArgs> queryResponder = createQueryResponder();

            this.Dispatcher.Invoke(delegate //main thread
            {
                InitializeComponent();
                //this.Show();

                FSBL.RouterClient.AddResponder(queryResponderChannel, createQueryResponder());
                FSBL.Logger.System.Log(new JToken[] { "responder added", queryResponderChannel });
                Trace.TraceError("responder added");
                FSBL.RouterClient.AddListener(listenerChannel, wrapListenerHandler(listenerChannel, createListenerHandler()));
                Trace.TraceError("listener added");
                FSBL.Logger.System.Log(new JToken[] { "listeners added", listenerChannel });
            });
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                FSBL.Dispose();
            } catch (Exception exception)
            {
                // No op
            }
        }

    }

}
