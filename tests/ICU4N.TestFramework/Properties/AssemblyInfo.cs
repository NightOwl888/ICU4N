using ICU4N;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ICU4N.Tests, PublicKey=" + AssemblyKeys.PublicKey)]
[assembly: InternalsVisibleTo("ICU4N.Tests.Transliterator, PublicKey = " + AssemblyKeys.PublicKey)]

[assembly: SuppressMessage("Microsoft.Design", "CA1034", Justification = "We don't care about Java-style classes in tests")]
