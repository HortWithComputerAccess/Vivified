using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleJSON;

public class JSONParseException : Exception
{
	private readonly string _error;

	public string TokenLocation { get; private set; }

	public string ParsedValue { get; private set; }

	public JSONParseException(string error, Stack<string> tokenLocation, string parsedValue)
		: base(error + " at location \"" + string.Join(".", tokenLocation.Reverse()) + "\"")
	{
		_error = error;
		TokenLocation = string.Join(".", tokenLocation.Reverse());
		ParsedValue = parsedValue;
	}

	public string ToUIFriendlyString()
	{
		return "JSON Parse Error: " + _error + ".\nError occured near node \"" + TokenLocation + "\", with a parsed value of " + ParsedValue + ".";
	}
}
