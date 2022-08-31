using Nml.Improve.Me.Dependencies;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using rampike.ocellar.Shared;

namespace rampike.ocellar.Manager
{
    public class ActivatedStateManager : IGeneratorManager
    {
        private Application _app;
        private string _uri;
        private IPathProvider _templatePathProvider;
        public IViewGenerator View_Generator;
        internal readonly IConfiguration _configuration;
        private readonly IPdfGenerator _pdfGenerator;
        public ActivatedStateManager(Application app,
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
            string path = _templatePathProvider.Get("ActivatedApplication");
            ActivatedApplicationViewModel vm = new ActivatedApplicationViewModel
            {
                ReferenceNumber = _app.ReferenceNumber,
                State = _app.State.ToDescription(),
                FullName = $"{_app.Person.FirstName} {_app.Person.Surname}",
                LegalEntity = _app.IsLegalEntity ? _app.LegalEntity : null,
                PortfolioFunds = _app.Products.SelectMany(p => p.Funds),
                PortfolioTotalAmount = _app.Products.SelectMany(p => p.Funds)
                                                .Select(f => (f.Amount - f.Fees) * _configuration.TaxRate)
                                                .Sum(),
                AppliedOn = _app.Date,
                SupportEmail = _configuration.SupportEmail,
                Signature = _configuration.Signature
            };
            view = View_Generator.GenerateFromPath(_uri + path, vm);

            PdfOptions pdfOptions = pdfLayoutSetup.SetPdfLayout();

            return _pdfGenerator.GenerateFromHtml(view, pdfOptions);
        }
    }
}
