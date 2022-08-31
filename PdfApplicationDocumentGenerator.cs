using System;
using System.Linq;
using Nml.Improve.Me.Dependencies;
using rampike.ocellar.Manager;

namespace Nml.Improve.Me
{
	public class PdfApplicationDocumentGenerator : IApplicationDocumentGenerator
	{
		private readonly IDataContext DataContext;
		private IPathProvider _templatePathProvider;
		public IViewGenerator View_Generator;
		internal readonly IConfiguration _configuration;
		private readonly ILogger<PdfApplicationDocumentGenerator> _logger;
		private readonly IPdfGenerator _pdfGenerator;

		public PdfApplicationDocumentGenerator(
			IDataContext dataContext,
			IPathProvider templatePathProvider,
			IViewGenerator viewGenerator,
			IConfiguration configuration,
			IPdfGenerator pdfGenerator,
			ILogger<PdfApplicationDocumentGenerator> logger)
		{
			if (dataContext != null)
				throw new ArgumentNullException(nameof(dataContext));
			
			DataContext = dataContext;
			_templatePathProvider = templatePathProvider ?? throw new ArgumentNullException("templatePathProvider");
			View_Generator = viewGenerator;
			_configuration = configuration;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_pdfGenerator = pdfGenerator;
		}

        public byte[] Generate(Guid applicationId, string baseUri)
        {
            //using factory pattern in order to vreate the instance ==> depending on application states
            //IGeneratorManager interface which act as sinngle responsibiliy which the main purpose is to generate pdf content

            IGeneratorManager _iGeneratorManager = GetGeneratorManager(applicationId, baseUri);
            return _iGeneratorManager != null ? _iGeneratorManager.generatePdfContent().ToBytes() : null;
        }

        public IGeneratorManager GetGeneratorManager(Guid appId, string Uri)
        {
            Application application = DataContext.Applications.Single(app => app.Id == appId);

            IGeneratorManager _iGeneratorManager = null;
            if (application != null)
            {
                if (Uri.EndsWith("/"))
                    Uri = Uri.Substring(Uri.Length - 1);

                //the object is defined and extended to subclasses
                //content genreated is diff in each subclasses.

                if (application.State == ApplicationState.Pending)
                    _iGeneratorManager = new PendingStateManager(application, Uri, _templatePathProvider, View_Generator, _configuration, _pdfGenerator);
                else if (application.State == ApplicationState.Activated)
                    _iGeneratorManager = new ActivatedStateManager(application, Uri, _templatePathProvider, View_Generator, _configuration, _pdfGenerator);
                else if (application.State == ApplicationState.InReview)
                    _iGeneratorManager = new InReviewStateManager(application, Uri, _templatePathProvider, View_Generator, _configuration, _pdfGenerator);
                else
                {
                    _logger.LogWarning(
                            $"The application is in state '{application.State}' and no valid document can be generated for it.");
                }
            }
            else
            {
                _logger.LogWarning(
                    $"No application found for id '{appId}'");
            }

            return _iGeneratorManager;

        }
    }
}
