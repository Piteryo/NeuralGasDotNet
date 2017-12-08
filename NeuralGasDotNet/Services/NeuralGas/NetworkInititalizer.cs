using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralGasDotNet.Services.NeuralGas
{
    class NetworkInititalizer
    {
        public NetworkInititalizer()
        {
            gng.fit(X, epoch_stride);
            render_scatter_gng(W, X, C, title: label + " epoch " + str(epoch_stride), show: false);
            gng.fit_p(create_dynamic_two_simple_blobs_fn(border_freq: 0, 005.0));

        }
    }
}
