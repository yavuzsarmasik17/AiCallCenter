namespace AiCallCenter.Application.Common.Telephony;

public sealed class TelephonyOptions
{
    public const string SectionName = "Telephony";

    public string Provider { get; set; }
        = TelephonyProviderName.Mock;

    public string DefaultFromNumber { get; set; }
        = string.Empty;

    public string PublicBaseUrl { get; set; }
        = string.Empty;

    public MockTelephonyOptions Mock { get; set; }
        = new();

    public TwilioTelephonyOptions Twilio { get; set; }
        = new();

    public NetgsmTelephonyOptions Netgsm { get; set; }
        = new();

    public BulutfonTelephonyOptions Bulutfon { get; set; }
        = new();
}

public sealed class MockTelephonyOptions
{
    public int RingingDelaySeconds { get; set; } = 2;

    public bool AutoComplete { get; set; }

    public int AutoCompleteDelaySeconds { get; set; } = 10;
}

public sealed class TwilioTelephonyOptions
{
    public string AccountSid { get; set; }
        = string.Empty;

    public string AuthToken { get; set; }
        = string.Empty;

    public string FromNumber { get; set; }
        = string.Empty;
}

public sealed class NetgsmTelephonyOptions
{
    public string Username { get; set; }
        = string.Empty;

    public string Password { get; set; }
        = string.Empty;

    public string Header { get; set; }
        = string.Empty;
}

public sealed class BulutfonTelephonyOptions
{
    public string ApiKey { get; set; }
        = string.Empty;

    public string Extension { get; set; }
        = string.Empty;
}
