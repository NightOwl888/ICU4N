using ICU4N;
using System;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(true)]

[assembly: InternalsVisibleTo("ICU4N.Collation, PublicKey=" + AssemblyKeys.PublicKey)]
[assembly: InternalsVisibleTo("ICU4N.CurrencyData, PublicKey=" + AssemblyKeys.PublicKey)]
[assembly: InternalsVisibleTo("ICU4N.Transliterator, PublicKey=" + AssemblyKeys.PublicKey)]

[assembly: InternalsVisibleTo("ICU4N.TestFramework, PublicKey=" + AssemblyKeys.PublicKey)]
[assembly: InternalsVisibleTo("ICU4N.Tests, PublicKey=" + AssemblyKeys.PublicKey)]
[assembly: InternalsVisibleTo("ICU4N.Tests.Collation, PublicKey=" + AssemblyKeys.PublicKey)]
[assembly: InternalsVisibleTo("ICU4N.Tests.Transliterator, PublicKey=" + AssemblyKeys.PublicKey)]
