using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LocalizationManager
{
	public static Locale_Base activeLocale;

	public static string GetActiveLocaleTranslator()
	{
		return Translate("txt-translator") + (activeLocale.translator ?? Translate("txt-unknown translator"));
	}

	public static string Translate(string p_key)
	{
		if (activeLocale.keyValuePairs.TryGetValue(p_key, out var value))
		{
			return value;
		}
		if (new Locale_EN().keyValuePairs.TryGetValue(p_key, out var value2))
		{
			return value2;
		}
		return p_key;
	}

	public static string GetLocalizedDate(DateTime p_customDateTime)
	{
		return string.Format(GetCultureInfo(), "{0:d MMMM yyyy}", p_customDateTime);
	}

	public static string GetLocalizedShortDate(DateTime p_customDateTime)
	{
		return string.Format(GetCultureInfo(), "{0:MMMM yyyy}", p_customDateTime);
	}

	public static string GetLocalizedIdeology(string p_ideologyName)
	{
		string text = Translate(p_ideologyName);
		if (text == "No translation")
		{
			return p_ideologyName;
		}
		return text;
	}

	public static string GetLocalizedFaction(string p_factionName)
	{
		string text = Translate(p_factionName);
		if (text == "No translation")
		{
			return p_factionName;
		}
		return text;
	}

	public static string GetLocalizedCampaignSide(string p_countryName)
	{
		if (p_countryName == "Nazi Germany" || p_countryName == "Japan")
		{
			return Translate("txt-axis side");
		}
		return Translate("txt-allies side");
	}

	public static string GetLocalizedScenario(string p_scenarioName)
	{
		if (!p_scenarioName.EndsWith(".sav"))
		{
			p_scenarioName += ".sav";
		}
		string text = Translate(p_scenarioName);
		if (text == p_scenarioName)
		{
			return Path.GetFileNameWithoutExtension(p_scenarioName);
		}
		return text;
	}

	public static string GetLocalizedScenario(FileInfo p_fileInfo)
	{
		string text = Translate(p_fileInfo.Name);
		if (text == "No translation")
		{
			return Path.GetFileNameWithoutExtension(p_fileInfo.FullName);
		}
		return text;
	}

	public static string GetLocalizedCountry(string p_countryName)
	{
		string text = Translate(p_countryName);
		if (text == "No translation")
		{
			if (ModsManager.GetModdedCountries(p_sort: false).Contains(p_countryName))
			{
				return p_countryName;
			}
			Debug.LogError(p_countryName + " is missing a translation in " + PlayerSettings.Instance.Language);
			return p_countryName;
		}
		return text;
	}

	public static string GetPolicyName(Policy p_policy)
	{
		return Translate("policy.name." + p_policy.Name);
	}

	public static string GetPolicyDescription(Policy p_policy)
	{
		return Translate("policy.description." + p_policy.Name);
	}

	public static GameLanguages GetSupportedLanguageCode()
	{
		switch (Application.systemLanguage)
		{
		default:
			return GameLanguages.EN;
		case SystemLanguage.French:
			return GameLanguages.FR;
		case SystemLanguage.English:
			return GameLanguages.EN;
		case SystemLanguage.Russian:
			return GameLanguages.RU;
		case SystemLanguage.Spanish:
			return GameLanguages.ES;
		case SystemLanguage.Italian:
			return GameLanguages.IT;
		case SystemLanguage.German:
			return GameLanguages.DE;
		case SystemLanguage.Polish:
			return GameLanguages.PL;
		case SystemLanguage.Korean:
			return GameLanguages.KR;
		case SystemLanguage.Greek:
			return GameLanguages.GR;
		case SystemLanguage.Dutch:
			return GameLanguages.NL;
		case SystemLanguage.Swedish:
			return GameLanguages.SE;
		case SystemLanguage.Czech:
			return GameLanguages.CZ;
		case SystemLanguage.Norwegian:
			return GameLanguages.NO;
		case SystemLanguage.Turkish:
			return GameLanguages.TR;
		case SystemLanguage.Catalan:
			return GameLanguages.CA;
		case SystemLanguage.Portuguese:
			return GameLanguages.PT;
		case SystemLanguage.Finnish:
			return GameLanguages.FI;
		case SystemLanguage.Ukrainian:
			return GameLanguages.UA;
		case SystemLanguage.Chinese:
		case SystemLanguage.ChineseSimplified:
			return GameLanguages.CNS;
		case SystemLanguage.ChineseTraditional:
			return GameLanguages.CNT;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void SetLanguage(GameLanguages p_gameLanguage)
	{
		SetActiveLocale(GetLocale(p_gameLanguage));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void SetActiveLocale(Locale_Base p_locale)
	{
		activeLocale = p_locale;
	}

	public static Locale_Base GetLocale(GameLanguages p_gameLanguage)
	{
		switch (p_gameLanguage)
		{
		case GameLanguages.EN:
			return new Locale_EN();
		case GameLanguages.FR:
			return new Locale_FR();
		case GameLanguages.IT:
			return new Locale_IT();
		case GameLanguages.RU:
			return new Locale_RU();
		case GameLanguages.DE:
			return new Locale_DE();
		case GameLanguages.ES:
			return new Locale_ES();
		case GameLanguages.GR:
			return new Locale_GR();
		case GameLanguages.NL:
			return new Locale_NL();
		case GameLanguages.PL:
			return new Locale_PL();
		case GameLanguages.PT:
			return new Locale_PT();
		case GameLanguages.FI:
			return new Locale_FI();
		case GameLanguages.SE:
			return new Locale_SE();
		case GameLanguages.CZ:
			return new Locale_CZ();
		case GameLanguages.TR:
			return new Locale_TR();
		case GameLanguages.JP:
			return new Locale_JP();
		case GameLanguages.KR:
			return new Locale_KR();
		case GameLanguages.NO:
			return new Locale_NO();
		case GameLanguages.CA:
			return new Locale_CA();
		case GameLanguages.BR:
			return new Locale_BR();
		case GameLanguages.UA:
			return new Locale_UA();
		case GameLanguages.CNS:
			return new Locale_CNS();
		case GameLanguages.CNT:
			return new Locale_CNT();
		default:
			return new Locale_EN();
		}
	}

	public static CultureInfo GetCultureInfo()
	{
		switch (PlayerSettings.Instance.Language)
		{
		default:
			return new CultureInfo("en-US");
		case GameLanguages.FR:
			return new CultureInfo("fr-FR");
		case GameLanguages.DE:
			return new CultureInfo("de-DE");
		case GameLanguages.PT:
			return new CultureInfo("pt-PT");
		case GameLanguages.IT:
			return new CultureInfo("it-IT");
		case GameLanguages.RU:
			return new CultureInfo("ru-RU");
		case GameLanguages.KR:
			return new CultureInfo("ko-KR");
		case GameLanguages.JP:
			return new CultureInfo("ja-JP");
		case GameLanguages.PL:
			return new CultureInfo("pl-PL");
		case GameLanguages.FI:
			return new CultureInfo("fi-FI");
		case GameLanguages.SE:
			return new CultureInfo("sv-SE");
		case GameLanguages.TR:
			return new CultureInfo("tr-TR");
		case GameLanguages.ES:
			return new CultureInfo("es-ES");
		case GameLanguages.GR:
			return new CultureInfo("el-GR");
		case GameLanguages.CZ:
			return new CultureInfo("cs-CZ");
		case GameLanguages.CNS:
			return new CultureInfo("zh-Hans");
		case GameLanguages.CNT:
			return new CultureInfo("zh-Hant");
		case GameLanguages.NL:
			return new CultureInfo("nl-NL");
		case GameLanguages.NO:
			return new CultureInfo("no-NO");
		case GameLanguages.CA:
			return new CultureInfo("ca-ES");
		case GameLanguages.BR:
			return new CultureInfo("pt-BR");
		case GameLanguages.UA:
			return new CultureInfo("uk-UA");
		}
	}
}
