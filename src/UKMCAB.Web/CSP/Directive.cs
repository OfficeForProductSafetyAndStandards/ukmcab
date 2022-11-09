using System.Text;

namespace UKMCAB.Web.CSP;

public abstract class Directive : IEquatable<Directive>
{
    private static readonly string Separator = ";";

    private readonly string _key;
    private readonly IList<string> _value;

    public Directive(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(nameof(key));
        }

        _key = key;
        _value = new List<string>();
    }

    public string Key
    {
        get
        {
            return _key;
        }
    }

    public IList<string> Value
    {
        get
        {
            return _value;
        }
    }

    public virtual string Compose()
    {
        var sb = new StringBuilder(_key);

        foreach (var item in _value)
        {
            sb.Append($" {item}");
        }

        sb.Append($"{Separator} ");

        return sb.ToString();
    }

    public bool Equals(Directive other) => other != null && _key == other.Key;

    public override bool Equals(object obj)
    {
        return Equals(obj as Directive);
    }

    public override int GetHashCode() => _value.GetHashCode();

    public void Merge(Directive directive)
    {
        if (directive == null)
        {
            throw new ArgumentNullException(nameof(directive));
        }

        if (!directive.Equals(this))
        {
            throw new ArgumentException(nameof(directive));
        }

        AddValues(directive.Value);
    }

    protected virtual void AddValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value == CspConstants.NoneKeyword)
        {
            _value.Clear();
            _value.Add(CspConstants.NoneKeyword);
        }
        else
        {
            _value.Remove(CspConstants.NoneKeyword);
            _value.Add(value);
        }
    }

    protected virtual void AddValues(IList<string> values)
    {
        foreach (var value in values)
        {
            AddValue(value);
        }
    }
    protected virtual void ClearValue()
    {
        _value.Clear();
    }
}