
--this file does various initialization functions for the lua environment
--these are not related to even the most basic game rules, but are instead just internal lua things
--it should be run before loading any sets

--seed random with time, for proper randoms
--apparently, cannot do this in the current version of NeoLua
math.randomseed(12)

--important for the functioning of the namespace function
--and also to provide a consistent identifier for the current namespace
--in most cases you should use this rather than _G
this = _G
name = "global"
global = this

--returns a namespace, named name, which is the child of the current namespace
--and more importantly creates this namespace if it does not exist
--if a name is not found in the current namespace, its parent namespace will be searched for this name, and so on all the way up to global
function namespace(name : string, parent)
	
	if rawget(parent, name) == nil then
		Log("Creating namespace " .. name .. " under " .. parent.name)

		local space = { }
		space.name = name  --mostly for debug purposes
		space.this = space
		space.meta = { }
		space.meta.__index = parent
		setmetatable(space, space.meta)
		space.parent = parent
		parent[name] = space
	end

	return parent[name]
end