function Main()
	print("luaTest")
	UITest.instance.txt = "luaTest";	
end

function OnApplicationQuit()
	print("OnApplicationQuit");
end