using Nml.Improve.Me.Dependencies;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using rampike.ocellar.Shared;

namespace rampike.ocellar.Manager
{
    public class InReviewStateManager : IGeneratorManager
    {
        private Application _app;
        private string _uri;
        private IPathProvider _templatePathProvider;
        public IViewGenerator View_Generator;
        internal readonly IConfiguration _configuration;
        private readonly IPdfGenerator _pdfGenerator;
        public InReviewStateManager(Application app,
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

            string path = _templatePathProvider.Get("InReviewApplication");
            string inReviewMessage = "Your application has been placed in review" +
                                _app.CurrentReview.Reason switch
                                {
                                    { } reason when reason.Contains("address") =>
                                        " pending outstanding address verification for FICA purposes.",
                                    { } reason when reason.Contains("bank") =>
                                        " pending outstanding bank account verification.",
                                    _ =>
                                        " because of suspicious account behaviour. Please contact support ASAP."
                                };
            InReviewApplicationViewModel vm = new InReviewApplicationViewModel();
            vm.ReferenceNumber = _app.ReferenceNumber;
            vm.State = _app.State.ToDescription();
            vm.FullName = string.Format("{0} {1}", _app.Person.FirstName, _app.Person.Surname);
            vm.LegalEntity = _app.IsLegalEntity ? _app.LegalEntity : null;
            vm.PortfolioFunds = _app.Products.SelectMany(p => p.Funds);
            vm.PortfolioTotalAmount = _app.Products.SelectMany(p => p.Funds)
                .Select(f => (f.Amount - f.Fees) * _configuration.TaxRate)
                .Sum();
            vm.InReviewMessage = inReviewMessage;
            vm.InReviewInformation = _app.CurrentReview;
            vm.AppliedOn = _app.Date;
            vm.SupportEmail = _configuration.SupportEmail;
            vm.Signature = _configuration.Signature;

            view = View_Generator.GenerateFromPath(string.Format("{0}{1}", _uri, path), vm);

            PdfOptions pdfOptions = pdfLayoutSetup.SetPdfLayout();

            return _pdfGenerator.GenerateFromHtml(view, pdfOptions);
        }
    }
}
