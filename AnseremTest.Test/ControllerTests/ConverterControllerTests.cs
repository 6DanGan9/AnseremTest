using AnseremTest.Controllers;
using AnseremTest.Models;
using System.Reflection;

namespace AnseremTest.Tests.ControllerTests
{
    public class ConverterControllerTests
    {
        private const string SwitchAddress = "v.vladislavovich@alfa.com";

        // ---- helpers ----

        private static List<Address> A(params string[] names)
            => names.Select(n => new Address(n)).ToList();

        private static Domain MakeDomain(string name, string[] stopList = null, string[] switchList = null)
        {
            var domain = new Domain(name);
            if (stopList != null)
                domain.StopList.AddRange(A(stopList));
            if (switchList != null)
                domain.SwitchList.AddRange(A(switchList));
            return domain;
        }

        private static List<string> InvokeGetResultCopy(
            List<Address> to, List<Address> copy, List<Domain> domains)
        {
            var target = new ConverterController(null, null);
            var method = target.GetType().GetMethod(
                "GetResultCopy",
                BindingFlags.NonPublic | BindingFlags.Static);

            var result = (List<Address>)method.Invoke(target, new object[] { to, copy, domains });
            return result.Select(a => a.Name).ToList();
        }

        private static void AssertCopy(List<string> actual, params string[] expected)
            => Assert.Equal(expected, actual);


        [Fact]
        public void AddressAdded_WhenDomainMatchesAndNoStopHits()
        {
            var to = A("q.qweshnikov@batut.com", "w.petrov@alfa.com");
            var copy = A("f.patit@buisness.com");
            var domains = new List<Domain>
                {
                    MakeDomain("alfa.com", switchList: new[] { SwitchAddress })
                };

            var result = InvokeGetResultCopy(to, copy, domains);

            AssertCopy(result, "f.patit@buisness.com", SwitchAddress);
        }

        [Fact]
        public void AddressNotAdded_WhenStopListHit()
        {
            var to = A("t.kogni@acl.com");
            var copy = A("i.ivanov@tbank.ru");
            var domains = new List<Domain>
        {
            MakeDomain("acl.com",
                stopList: new[] { "i.ivanov@tbank.ru" },
                switchList: new[] { SwitchAddress })
        };

            var result = InvokeGetResultCopy(to, copy, domains);

            AssertCopy(result, "i.ivanov@tbank.ru");
        }

        [Fact]
        public void AddressesRemoved_WhenStopListHit()
        {
            var to = A("t.kogni@acl.com", "i.ivanov@tbank.ru");
            var copy = A("e.gras@tbank.ru", "t.tbankovich@tbank.ru", "v.veronickovna@tbank.ru");
            var domains = new List<Domain>
        {
            MakeDomain("tbank.ru",
                stopList: new[] { "i.ivanov@tbank.ru" },
                switchList: new[] { "t.tbankovich@tbank.ru", "v.veronickovna@tbank.ru" })
        };

            var result = InvokeGetResultCopy(to, copy, domains);

            AssertCopy(result, "e.gras@tbank.ru");
        }

        [Fact]
        public void NothingChanges_WhenNoDomains()
        {
            var to = A("z.xcy@email.com");
            var copy = A("p.rivet@email.com");

            var result = InvokeGetResultCopy(to, copy, new List<Domain>());

            AssertCopy(result, "p.rivet@email.com");
        }

        [Fact]
        public void NothingChanges_WhenDomainNotAmongRecipients()
        {
            // Правило для alfa.com, но ни один получатель не с этого домена → continue
            var to = A("z.xcy@email.com");
            var copy = A("p.rivet@email.com");
            var domains = new List<Domain>
        {
            MakeDomain("alfa.com", switchList: new[] { SwitchAddress })
        };

            var result = InvokeGetResultCopy(to, copy, domains);

            AssertCopy(result, "p.rivet@email.com");
        }

        [Fact]
        public void SwitchAddressNotAddedTwice_WhenAlreadyInCopy()
        {
            var to = A("w.petrov@alfa.com");
            var copy = A(SwitchAddress);
            var domains = new List<Domain>
        {
            MakeDomain("alfa.com", switchList: new[] { SwitchAddress })
        };

            var result = InvokeGetResultCopy(to, copy, domains);

            AssertCopy(result, SwitchAddress);
        }

        [Fact]
        public void OriginalCopyList_IsNotMutated()
        {
            var to = A("w.petrov@alfa.com");
            var copy = A("f.patit@buisness.com");
            var domains = new List<Domain>
        {
            MakeDomain("alfa.com", switchList: new[] { SwitchAddress })
        };

            InvokeGetResultCopy(to, copy, domains);

            Assert.Single(copy);
            Assert.Equal("f.patit@buisness.com", copy[0].Name);
        }

        [Fact]
        public void Matching_IsCaseInsensitive()
        {
            var to = A("w.petrov@ALFA.COM");
            var copy = A("f.patit@buisness.com");
            var domains = new List<Domain>
        {
            MakeDomain("Alfa.Com", switchList: new[] { SwitchAddress })
        };

            var result = InvokeGetResultCopy(to, copy, domains);

            AssertCopy(result, "f.patit@buisness.com", SwitchAddress);
        }
    }
}
