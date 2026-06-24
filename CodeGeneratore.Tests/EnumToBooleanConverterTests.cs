using Code_Generatore.BusinessLayer;
using Code_Generatore.Converters;
using System.Globalization;
using System.Windows.Data;

namespace CodeGeneratore.Tests;

public class EnumToBooleanConverterTests
{
    private readonly EnumToBooleanConverter _converter;

    public EnumToBooleanConverterTests()
    {
        _converter = new EnumToBooleanConverter();
    }

    [Theory]
    [InlineData(ProjectGeneratore.enProjectType.WINDOWS_FORMS, "WINDOWS_FORMS", true)]
    [InlineData(ProjectGeneratore.enProjectType.WINDOWS_FORMS, "WinForms", false)]
    [InlineData(ProjectGeneratore.enProjectType.WINDOWS_PRESENTATION_FOUNDATION, "WINDOWS_PRESENTATION_FOUNDATION", true)]
    [InlineData(ProjectGeneratore.enProjectType.WINDOWS_PRESENTATION_FOUNDATION, "WPF", false)]
    public void Convert_ShouldReturnExpectedResult(
        ProjectGeneratore.enProjectType value,
        string parameter,
        bool expected)
    {
        var result = _converter.Convert(value, typeof(bool), parameter, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_ShouldReturnFalse_WhenValueIsNull()
    {
        var result = _converter.Convert(null, typeof(bool), "WINDOWS_FORMS", CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_ShouldReturnFalse_WhenParameterIsNull()
    {
        var result = _converter.Convert(
            ProjectGeneratore.enProjectType.WINDOWS_FORMS,
            typeof(bool),
            null,
            CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_ShouldReturnTrue_WhenNonEnumValueMatchesParameterString()
    {
        var result = _converter.Convert(42, typeof(bool), "42", CultureInfo.InvariantCulture);
        Assert.Equal(true, result);
    }

    [Theory]
    [InlineData(true, "WINDOWS_FORMS", ProjectGeneratore.enProjectType.WINDOWS_FORMS)]
    [InlineData(true, "WINDOWS_PRESENTATION_FOUNDATION", ProjectGeneratore.enProjectType.WINDOWS_PRESENTATION_FOUNDATION)]
    public void ConvertBack_ShouldReturnEnumValue_WhenChecked
        (
        bool value,
        string parameter,
        ProjectGeneratore.enProjectType expected
        )
    {
        var result = _converter.ConvertBack(value, typeof(ProjectGeneratore.enProjectType), parameter, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertBack_ShouldThrowException_WhenParamaterInvalid()
    {
        Assert.Throws<ArgumentException>(
            () =>
                _converter.ConvertBack(true, typeof(ProjectGeneratore.enProjectType), "INVALID_PARAMTER", CultureInfo.InvariantCulture)
            ); 
    }

    [Fact]
    public void ConvertBack_ShouldThrowException_WhenValueIsNotBoolean()
    {
        Assert.Throws<ArgumentException>(
            () =>
                _converter.ConvertBack("string", typeof(ProjectGeneratore.enProjectType), "WINDOWS_FORMS", CultureInfo.InvariantCulture)
            );
    }

    [Theory]
    [InlineData(false, "WINDOWS_FORMS")]
    [InlineData(false, "WINDOWS_PRESENTATION_FOUNDATION")]
    public void ConvertBack_ShouldReturnBindingDoNothing_WhenUnchecked (bool value, string parameter)
    {
        var result = _converter.ConvertBack(value, typeof(ProjectGeneratore.enProjectType), parameter, CultureInfo.InvariantCulture);

        Assert.Same(Binding.DoNothing, result);
    }

    [Fact]
    public void ConvertBack_ShouldThrowException_WhenParameterIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => _converter.ConvertBack(true, typeof(ProjectGeneratore.enProjectType), null, CultureInfo.InvariantCulture)
        );
    }
}
