using Nml.Improve.Me.Dependencies;
using System;
using System.Collections.Generic;
using System.Text;

namespace rampike.ocellar.Shared
{
    public class pdfLayoutSetup
    {
        public static PdfOptions SetPdfLayout()
        {
            return new PdfOptions
            {
                PageNumbers = PageNumbers.Numeric,
                HeaderOptions = new HeaderOptions
                {
                    HeaderRepeat = HeaderRepeat.FirstPageOnly,
                    HeaderHtml = PdfConstants.Header
                }
            };

        }
    }
}
