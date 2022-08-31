using Nml.Improve.Me.Dependencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rampike.ocellar.Shared;

namespace rampike.ocellar.Manager
{
    public class PendingStateManager : IGeneratorManager
    {
        private Application _app;
        private string _uri;
        private IPathProvider _templatePathProvider;
        public IViewGenerator View_Generator;
        internal readonly IConfiguration _configuration;
        private readonly IPdfGenerator _pdfGenerator;
        public PendingStateManager(Application app,
            string uri,
            IPathProvider templatePathProvider,
            IViewGenerator viewGenerator,
            IConfiguration configuration,
            IPdfGenerator pdfGenerator)
        {
            _app = app;
            _uri = uri;
            _templatePathProvider = templatePathProvider ?? throw new ArgumentNullException("templatePathProvider");
            View_Generator = viewGenerator;
            _configuration = configuration;
            _pdfGenerator = pdfGenerator;
        }

        public PdfDocument generatePdfContent()
        {
            string view;
            string path = _templatePathProvider.Get("PendingApplication");
            PendingApplicationViewModel vm = new PendingApplicationViewModel
            {
                ReferenceNumber = _app.ReferenceNumber,
                State = _app.State.ToDescription(),
                FullName = _app.Person.FirstName + " " + _app.Person.Surname,
                AppliedOn = _app.Date,
                SupportEmail = _configuration.SupportEmail,
                Signature = _configuration.Signature
            };
            view = View_Generator.GenerateFromPath(string.Format("{0}{1}", _uri, path), vm);

            PdfOptions pdfOptions = pdfLayoutSetup.SetPdfLayout();

            return _pdfGenerator.GenerateFromHtml(view, pdfOptions);
        }
    }
}
