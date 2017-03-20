class_name = {}
function class(cname)
	return function(base)
		if type(base) == 'string' then
			base = _G[base]
			return function(body)
				_G[cname] = define_class(base, body)
				class_name[_G[cname]] = cname
			end
		elseif class_name[base] then
			return function(body)
				_G[cname] = define_class(base, body)
				class_name[_G[cname]] = cname
			end
		else
			_G[cname] = define_class(base)
			class_name[_G[cname]] = cname
		end
	end
end
do 
	local function init(self) end
	local function to_str(self)
		return string.format("<%s#0x%s>",class_name[self.class],self.id)
	end
	function define_class(base, body)
		if not body then
			body, base = base, body
		end
		local class = body
		class.base = base
		if base then setmetatable(class,{__index = base}) end
		class.init = class.init or init
		class.__meta = {
			__index = class,
			__add = class["+"],
			__sub = class["-"],
			__mul = class["*"],
			__div = class["/"],
			__mod = class["%"],
			__pow = class["^"],
			__unm = class["-@"],
			__concat = class[".."],
			__len = class["#"],
			__eq = class["=="],
			__lt = class["<"],
			__le = class["<="],
			__call = class["()"],
			__tostring = class.__tostring or to_str,
			__pairs = class.__pairs,
			__ipairs = class.__ipairs,
		}
		return class
	end
end
function new(class, ...)
	if type(class) == "string" then
		class = _G[class]
	end
	local obj = {class = class}
	obj.id = string.gsub(tostring(obj), "table: (%d*)", "%1")
	setmetatable(obj, class.__meta)
	obj:init(...)
	return obj
end