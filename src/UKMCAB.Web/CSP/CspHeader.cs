using Microsoft.AspNetCore.Http;

namespace UKMCAB.Web.CSP;

public class CspHeader
{
    private readonly List<Directive> _list;

    public CspHeader() => _list = new List<Directive>();

    public CspHeader Add(Directive directive)
    {
        ArgumentNullException.ThrowIfNull(directive);
        if (_list.Contains(directive))
        {
            _list[_list.IndexOf(directive)].Merge(directive);
        }
        else
        {
            _list.Add(directive);
        }
        return this;
    }

    public void AddHeader(IHeaderDictionary headers)
    {
        ArgumentNullException.ThrowIfNull(headers);
        if (_list.Count > 0)
        {
            var header = string.Join(string.Empty, _list.Select(x => x.Compose()));
            headers[CspConstants.CspHeader] = header;
        }
    }

    public CspHeader RemoveAll()
    {
        _list.Clear();
        return this;
    }
}