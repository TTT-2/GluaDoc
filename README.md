# GluaDoc
GluaDoc is a C# tool that allows you generating HTML/PHP documentation for your creations.
It's a massively modified version of the old simple, but limited LuaDocIt tool (https://github.com/Nethie/LuaDocIt/tree/20f6008f62ae5334c44a4b2e915a9eb482fe8810)

# Preview

Software: 

##### TODO

Generated HTML/PHP file: 

##### TODO

# How to:
To use GluaDoc, simply open your lua files one by one and above any function you want to document add any of the following lines:

```
---
-- (@desc) [Text]
-- @args [Type name], [Type name], [Type name]
-- @realm [Server/Client/Shared]
-- @note [Text]
-- @return [Type name]
-- @anythingyouwish [Anything]
```

Here's an example how to fill them:

```
---
-- Give player money
-- @args Player person, Integer amount, String message
-- @realm Server
-- @note Negative amount will take money instead
function giveMoney(person, amount, message)
  -- some code
end
```
