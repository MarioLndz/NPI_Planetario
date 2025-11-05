using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlanetTextCSVLoader : MonoBehaviour
{
    // ---------- Singleton ----------
    public static PlanetTextCSVLoader Instance { get; private set; }
    [Tooltip("Si está activo, este loader persiste entre escenas.")]
    public bool dontDestroyOnLoad = true;

    // ---------- Config ----------
    [Header("Fuente de datos (usa UNO de los dos)")]
    public TextAsset csvFile;                         // Arrástralo aquí
    public string resourcesPath = "planet_texts_multi"; // O pon el CSV en Resources/planet_texts_multi.csv

    [Header("Idioma por defecto (debe coincidir con la cabecera)")]
    public string defaultLanguage = "SPANISH";

    // ---------- Datos ----------
    // id -> (lang -> text)
    public Dictionary<string, Dictionary<string, string>> db
        = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

    // Idiomas detectados en cabecera (en orden)
    public List<string> languages = new List<string>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        Load();
    }

    // ---------- API ----------
    public static string SGet(string id) => Instance ? Instance.GetText(id) : id;
    public static string SGet(string id, string lang) => Instance ? Instance.GetText(id, lang) : id;

    public string GetText(string id) => GetText(id, defaultLanguage);

    public string GetText(string id, string language)
    {
        if (string.IsNullOrEmpty(id)) return "";
        if (!db.TryGetValue(id, out var perLang) || perLang == null || perLang.Count == 0) return id;

        // 1) idioma pedido
        if (!string.IsNullOrEmpty(language) &&
            perLang.TryGetValue(language, out var t) && !string.IsNullOrEmpty(t)) return t;

        // 2) idioma por defecto
        if (!string.IsNullOrEmpty(defaultLanguage) &&
            perLang.TryGetValue(defaultLanguage, out var td) && !string.IsNullOrEmpty(td)) return td;

        // 3) primer idioma disponible
        foreach (var lang in languages)
            if (perLang.TryGetValue(lang, out var any) && !string.IsNullOrEmpty(any)) return any;

        return id;
    }

    [ContextMenu("Reload CSV")]
    public void Load()
    {
        string csv = null;

        if (csvFile != null) csv = csvFile.text;
        else
        {
            var ta = Resources.Load<TextAsset>(resourcesPath);
            if (ta != null) csv = ta.text;
        }

        if (string.IsNullOrEmpty(csv))
        {
            Debug.LogError("[PlanetTextCSVLoader] No se encontró CSV (asigna TextAsset o usa Resources).");
            db.Clear(); languages.Clear();
            return;
        }

        Parse(csv); // ; fijo
#if UNITY_EDITOR
        Debug.Log($"[PlanetTextCSVLoader] OK. IDs={db.Count}. Idiomas=[{string.Join(", ", languages)}]. Default={defaultLanguage}");
#endif
    }

    // ---------- Parser (delimitador ';' fijo, soporta comillas) ----------
    private void Parse(string csv)
    {
        db.Clear();
        languages.Clear();

        int i = 0;
        var header = ReadLine(csv, ref i); // delim ';'
        if (header.Count < 2)
        {
            Debug.LogError("[PlanetTextCSVLoader] Cabecera inválida. Esperado: ID;SPANISH;ENGLISH;...");
            return;
        }

        if (!header[0].Equals("ID", StringComparison.OrdinalIgnoreCase))
            Debug.LogWarning($"[PlanetTextCSVLoader] Primera columna no es 'ID' sino '{header[0]}'. Se usará igualmente.");

        for (int c = 1; c < header.Count; c++)
        {
            var lang = header[c]?.Trim();
            if (!string.IsNullOrEmpty(lang)) languages.Add(lang);
        }

        while (i < csv.Length)
        {
            var row = ReadLine(csv, ref i);
            if (row.Count == 0) continue;

            string id = row.Count > 0 ? row[0]?.Trim() : null;
            if (string.IsNullOrEmpty(id)) continue;

            var perLang = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int c = 1; c < header.Count; c++)
            {
                string lang = header[c];
                string val = (c < row.Count) ? row[c] : "";
                perLang[lang] = val;
            }
            db[id] = perLang;
        }
    }

    private static List<string> ReadLine(string s, ref int i)
    {
        const char delimiter = ';';
        var fields = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        while (i < s.Length)
        {
            char ch = s[i++];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i < s.Length && s[i] == '"') { sb.Append('"'); i++; } // "" -> "
                    else inQuotes = false;
                }
                else sb.Append(ch);
            }
            else
            {
                if (ch == delimiter) { fields.Add(sb.ToString()); sb.Clear(); }
                else if (ch == '\r') { /* ignore */ }
                else if (ch == '\n') break;
                else if (ch == '"') inQuotes = true;
                else sb.Append(ch);
            }
        }
        fields.Add(sb.ToString());
        return fields;
    }
}
