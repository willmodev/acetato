using Acetato.Application.Diagnostics;
using FluentAssertions;
using Xunit;

namespace Acetato.Application.Tests;

/// <summary>Reporte para soporte (HU-33): sin el nombre de usuario ni el del equipo.</summary>
public sealed class ReportRedactorTests
{
    [Fact]
    public void Replaces_the_user_inside_paths()
    {
        ReportRedactor.Redact(@"C:\Users\ana\AppData\Local\Acetato\logs", "ana", "OFICINA")
            .Should().Be(@"C:\Users\<usuario>\AppData\Local\Acetato\logs");
    }

    [Fact]
    public void Replaces_paths_escaped_in_json()
    {
        ReportRedactor.Redact("""{ "ruta": "C:\Users\ana\Desktop" }""", "ana", "OFICINA")
            .Should().Be("""{ "ruta": "C:\Users\<usuario>\Desktop" }""");
    }

    [Fact]
    public void Replaces_whole_words_ignoring_case()
    {
        ReportRedactor.Redact("Equipo OFICINA, usuario Ana", "ana", "oficina")
            .Should().Be("Equipo <equipo>, usuario <usuario>");
    }

    [Theory]
    [InlineData("banana")]
    [InlineData("Mariana")]
    [InlineData("ana_2")]
    [InlineData("ana2")]
    public void Leaves_words_that_only_contain_a_short_name(string word)
    {
        ReportRedactor.Redact($"texto {word} texto", "ana", "PC").Should().Be($"texto {word} texto");
    }

    [Fact]
    public void A_machine_name_that_contains_the_user_is_the_machine()
    {
        ReportRedactor.Redact(@"\ANA-PC\compartida \Users\ana\", "ana", "ANA-PC")
            .Should().Be(@"\<equipo>\compartida \Users\<usuario>\");
    }

    [Fact]
    public void Names_with_regex_characters_are_taken_literally()
    {
        ReportRedactor.Redact(@"\Users\j.perez (2)\ y \Users\jxperez (2)\", "j.perez (2)", "PC")
            .Should().Be(@"\Users\<usuario>\ y \Users\jxperez (2)\");
    }

    [Fact]
    public void Empty_names_leave_the_text_as_is()
    {
        ReportRedactor.Redact("nada que cambiar", "", " ").Should().Be("nada que cambiar");
    }
}
