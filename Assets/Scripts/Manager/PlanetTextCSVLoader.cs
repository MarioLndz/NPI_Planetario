using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum Language
{
    Spanish = 0,
    English = 1,
    French = 2,
    German = 3
    // añade más si los usas
}

[Serializable]
public class PlanetPageData
{
    public string planetId;
    public int page;
    public string title;
    public string body;
}

public class PlanetTextCSVLoader : MonoBehaviour
{
    // ---------- Singleton ----------
    public static PlanetTextCSVLoader Instance { get; private set; }
    [Tooltip("Si está activo, este loader persiste entre escenas.")]
    public bool dontDestroyOnLoad = true;

    // ---------- Config (TEXTOS SIMPLES) ----------
    [Header("Texto genérico (multi-idioma)")]
    public TextAsset csvFile;                           // CSV antiguo: ID;SPANISH;ENGLISH;...
    public string resourcesPath = "planet_texts_multi"; // O pon el CSV en Resources/planet_texts_multi.csv

    // ==== NUEVO PÁGINAS ====
    [Header("Info por páginas de planetas")]
    [Tooltip("CSV tipo: ID;PAGE;SPANISH TITLE;SPANISH BODY;ENGLISH TITLE;ENGLISH BODY")]
    public TextAsset pagesCsvFile;                     // aquí arrastras prueba_planetas.csv
    // =======================

    [Header("Idioma por defecto")]
    public Language currentLanguage;
    private Language defaultLanguage = Language.Spanish;

    // ---------- Datos TEXTO SIMPLE ----------
    // id -> (Language -> text)
    public Dictionary<string, Dictionary<Language, string>> db
        = new Dictionary<string, Dictionary<Language, string>>(StringComparer.OrdinalIgnoreCase);

    // Idiomas detectados en cabecera (en orden)
    public List<Language> languages = new List<Language>();

    // ==== NUEVO PÁGINAS ====
    // key = planetId_language  (ej: "mercurio_Spanish") -> lista de páginas ordenadas
    private Dictionary<string, List<PlanetPageData>> pagesDb =
        new Dictionary<string, List<PlanetPageData>>(StringComparer.OrdinalIgnoreCase);

    private string PageKey(string planetId, Language lang)
        => planetId.ToLowerInvariant() + "_" + lang.ToString();
    // =======================

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        Load();         // textos simples
        LoadPages();    // ==== NUEVO: páginas ====

        currentLanguage = defaultLanguage;
    }

    // ---------- API TEXTO SIMPLE ----------
    public static string SGet(string id) => Instance ? Instance.GetText(id) : id;
    public static string SGet(string id, Language lang) => Instance ? Instance.GetText(id, lang) : id;

    [Obsolete("Usa SGet(id, Language)")]
    public static string SGet(string id, string lang) => Instance ? Instance.GetText(id, lang) : id;

    public string GetText(string id) => GetText(id, currentLanguage);

    public string GetInfo(PlanetClickable planet, int page)
    {
        // API antigua: mercurio_info_0, etc.
        string info_id = planet.GetId() + "_info_" + page;
        return GetText(info_id);
    }

    public string GetNombre(PlanetClickable p)
    {
        string nombre_id = p.GetId() + "_nombre";
        return GetText(nombre_id);
    }

    public string GetText(string id, Language language)
    {
        if (string.IsNullOrEmpty(id)) return "";
        if (!db.TryGetValue(id, out var perLang) || perLang == null || perLang.Count == 0) return id;

        // 1) idioma pedido
        if (perLang.TryGetValue(language, out var t) && !string.IsNullOrEmpty(t)) return t;

        // 2) idioma por defecto
        if (perLang.TryGetValue(defaultLanguage, out var td) && !string.IsNullOrEmpty(td)) return td;

        // 3) primer idioma disponible según cabecera
        foreach (var lang in languages)
            if (perLang.TryGetValue(lang, out var any) && !string.IsNullOrEmpty(any)) return any;

        return id;
    }

    // Compatibilidad con código antiguo que pasa string
    public string GetText(string id, string language)
    {
        if (TryParseLanguage(language, out var lang))
            return GetText(id, lang);
        return GetText(id); // fallback
    }

    public void setLanguage(Language new_language)
    {
        currentLanguage = new_language;
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
            Debug.LogError("[PlanetTextCSVLoader] No se encontró CSV de textos simples.");
            db.Clear(); languages.Clear();
            return;
        }

        Parse(csv); // ; fijo
#if UNITY_EDITOR
        Debug.Log($"[PlanetTextCSVLoader] TEXTOS OK. IDs={db.Count}. Idiomas=[{string.Join(", ", languages)}]. Default={defaultLanguage}");
#endif
    }

    // ---------- Parser TEXTO SIMPLE (delimitador ';' fijo, soporta comillas) ----------
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

        // Mapea columnas a enum Language
        var colToLang = new Dictionary<int, Language>();
        for (int c = 1; c < header.Count; c++)
        {
            var raw = header[c]?.Trim();
            if (TryParseLanguage(raw, out var lang))
            {
                colToLang[c] = lang;
                if (!languages.Contains(lang)) languages.Add(lang);
            }
            else
            {
                Debug.LogWarning($"[PlanetTextCSVLoader] Columna de idioma desconocida '{raw}' ignorada.");
            }
        }

        while (i < csv.Length)
        {
            var row = ReadLine(csv, ref i);
            if (row.Count == 0) continue;

            string id = row.Count > 0 ? row[0]?.Trim() : null;
            if (string.IsNullOrEmpty(id)) continue;

            var perLang = new Dictionary<Language, string>();
            foreach (var kv in colToLang)
            {
                int c = kv.Key;
                var lang = kv.Value;
                string val = (c < row.Count) ? row[c] : "";
                perLang[lang] = val;
            }
            db[id] = perLang;
        }
    }

    // ---------- ==== NUEVO: PARSER DE PÁGINAS ==== ----------
    /*private void LoadPages()
    {
        pagesDb.Clear();

        if (pagesCsvFile == null)
        {
            Debug.LogWarning("[PlanetTextCSVLoader] No hay CSV de páginas asignado (pagesCsvFile).");
            return;
        }

        var csv = pagesCsvFile.text;
        if (string.IsNullOrEmpty(csv)) return;

        int i = 0;
        var header = ReadLine(csv, ref i);
        // Esperado: ID;PAGE;SPANISH TITLE;SPANISH BODY;ENGLISH TITLE;ENGLISH BODY

        while (i < csv.Length)
        {
            var row = ReadLine(csv, ref i);
            if (row.Count == 0) continue;

            string id = row[0].Trim();
            if (string.IsNullOrEmpty(id)) continue;

            if (row.Count < 4)
            {
                Debug.LogWarning($"[PlanetTextCSVLoader] Fila de páginas con columnas insuficientes: {id}");
                continue;
            }

            // planetId = parte antes del primer '_'
            var idParts = id.Split('_');
            string planetId = idParts[0].Trim().ToLowerInvariant();

            int pageNum = 0;
            int.TryParse(row[1].Trim(), out pageNum);

            // Español
            var spanish = new PlanetPageData
            {
                planetId = planetId,
                page = pageNum,
                title = row[2].Trim(),
                body = row[3].Trim()
            };
            AddPage(spanish, Language.Spanish);

            // Inglés (si existe)
            if (row.Count >= 6)
            {
                var english = new PlanetPageData
                {
                    planetId = planetId,
                    page = pageNum,
                    title = row[4].Trim(),
                    body = row[5].Trim()
                };
                AddPage(english, Language.English);
            }
        }

        // Ordenar todas las listas por número de página
        foreach (var kv in pagesDb)
            kv.Value.Sort((a, b) => a.page.CompareTo(b.page));

#if UNITY_EDITOR
        Debug.Log($"[PlanetTextCSVLoader] PÁGINAS OK. Claves={pagesDb.Count}");
#endif
    }*/

    // ---------- ==== NUEVO: PARSER DE PÁGINAS SIMPLE ==== ----------
    private void LoadPages()
    {
        pagesDb.Clear();

        if (pagesCsvFile == null)
        {
            Debug.LogWarning("[PlanetTextCSVLoader] No hay CSV de páginas asignado (pagesCsvFile).");
            return;
        }

        var csv = pagesCsvFile.text;
        if (string.IsNullOrEmpty(csv)) return;

        // Separamos por líneas
        var lines = csv.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1) return; // solo cabecera

        // Saltamos la cabecera (línea 0)
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Por si quedan comillas al principio / final, las quitamos
            if (line.Length >= 2 && line[0] == '"' && line[line.Length - 1] == '"')
                line = line.Substring(1, line.Length - 2);

            var cols = line.Split(';');
            if (cols.Length < 4)
            {
                Debug.LogWarning($"[PlanetTextCSVLoader] Fila de páginas con columnas insuficientes: '{line}'");
                continue;
            }

            string id = cols[0].Trim();        // mercurio_page_1, tierra_page_2, etc.
            if (string.IsNullOrEmpty(id)) continue;

            // planetId = parte antes del primer '_'
            var idParts = id.Split('_');
            string planetId = idParts[0].Trim().ToLowerInvariant();

            int pageNum = 0;
            int.TryParse(cols[1].Trim(), out pageNum);

            // Español
            var spanish = new PlanetPageData
            {
                planetId = planetId,
                page = pageNum,
                title = cols[2].Trim(),
                body = cols[3].Trim()
            };
            AddPage(spanish, Language.Spanish);

            // Inglés (si existe)
            if (cols.Length >= 6)
            {
                var english = new PlanetPageData
                {
                    planetId = planetId,
                    page = pageNum,
                    title = cols[4].Trim(),
                    body = cols[5].Trim()
                };
                AddPage(english, Language.English);
            }
        }

        // Ordenar todas las listas por número de página
        foreach (var kv in pagesDb)
            kv.Value.Sort((a, b) => a.page.CompareTo(b.page));

        // DEBUG: ver qué claves tenemos
        foreach (var kv in pagesDb)
            Debug.Log($"[PlanetTextCSVLoader] key='{kv.Key}' pages={kv.Value.Count}");
    }


    private void AddPage(PlanetPageData data, Language lang)
    {
        var key = PageKey(data.planetId, lang);
        if (!pagesDb.TryGetValue(key, out var list))
        {
            list = new List<PlanetPageData>();
            pagesDb[key] = list;
        }
        list.Add(data);
    }

    public List<PlanetPageData> GetPlanetPages(string planetId)
    {
        return GetPlanetPages(planetId, currentLanguage);
    }

    public List<PlanetPageData> GetPlanetPages(string planetId, Language lang)
    {
        if (string.IsNullOrEmpty(planetId)) return null;
        var key = PageKey(planetId, lang);
        pagesDb.TryGetValue(key, out var list);
        return list;
    }
    // ---------- FIN PÁGINAS ----------

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

    // --------- Helpers idioma ---------
    public static bool TryParseLanguage(string s, out Language lang)
    {
        lang = default;
        if (string.IsNullOrWhiteSpace(s)) return false;

        // Normaliza
        s = s.Trim();

        // Aliases comunes
        switch (s.ToUpperInvariant())
        {
            case "ES":
            case "ES-ES":
            case "SPANISH":
            case "CASTILIAN":
                lang = Language.Spanish; return true;
            case "EN":
            case "EN-GB":
            case "EN-US":
            case "ENGLISH":
                lang = Language.English; return true;
            case "FR":
            case "FR-FR":
            case "FRENCH":
                lang = Language.French; return true;
            case "DE":
            case "DE-DE":
            case "GERMAN":
                lang = Language.German; return true;
        }

        // Fallback: usa nombres del enum (case-insensitive)
        return Enum.TryParse(s, true, out lang);
    }
}
