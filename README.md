# LuaDocIt

LuaDocIt is a simple tool that allows you to generate HTML/PHP documentation for your creations.

## Preview

Software:

![](https://s13.postimg.org/su4rst03b/image.png)

Generated HTML/PHP file:

![](https://s13.postimg.org/8o19tx4fr/image.png)

## How to

To use LuaDocIt, simply open your lua files one by one and above any function you want to document add any of the following lines:

```txt
--  @desc [Text];
--  @args [name Type], [name Type], [name Type];
--  @realm [Server/Client/Shared];
--  @note [Text];
--  @anythingyouwish [Anything];
```

Here's an example how to fill them:

```txt
--  @desc Give player money;
--  @args person Player, amount Integer, message String;
--  @realm Server;
--  @note Negative amount will take money instead;
function giveMoney( person, amount, message )
  --some code
end
```

## How to Set up the Webserver

Normally you only have to drop the `SVDATA` folder into your webserver. If you want to change some things, you have to edit the typescript files found in the `__workspace__` and recompile them. The following steps explain how to do so.

After installing node you have to navigate into the `webfiles` folder. Issue the command

```txt
npm install
```

inside the folder to set up all needed dependencies. This has to be done only once.

Every time you want to recompile the changed sourcecode, you first have to compile typescript into javascript and combining the combiled files into a single minimized javascript file. A new command was added to do this. Use

```txt
npm run compile
```

to create the new files. They are located in the `dist` folder.
