using System.Runtime.CompilerServices;

// This makes the internal classes of the Trading project visible to the Trading.Tests project.
// It is necessary for the WebApplicationFactory in the integration tests to work correctly.
[assembly: InternalsVisibleTo("Trading.Tests")]
