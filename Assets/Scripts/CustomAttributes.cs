using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LObjectPropertyAliasAsAttribute
	: Attribute
{
	public string Alias { get; private set; }

	public LObjectPropertyAliasAsAttribute(string alias)
	{
		Alias = alias;
	}
}

public class LuaFunctionAliasAsAttribute
	: Attribute
{
	public string Alias { get; private set; }

	public LuaFunctionAliasAsAttribute(string alias)
	{
		Alias = alias;
	}
}
