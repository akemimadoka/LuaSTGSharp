using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[AttributeUsage(AttributeTargets.Property)]
public class LObjectPropertyAliasAsAttribute
	: Attribute
{
	public string Alias { get; private set; }

	public LObjectPropertyAliasAsAttribute(string alias)
	{
		Alias = alias;
	}
}

[AttributeUsage(AttributeTargets.Method)]
public class LuaFunctionAliasAsAttribute
	: Attribute
{
	public string Alias { get; private set; }

	public LuaFunctionAliasAsAttribute(string alias)
	{
		Alias = alias;
	}
}
