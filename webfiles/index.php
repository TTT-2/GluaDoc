<?php
	require_once __DIR__ . '/bootstrap.php';
?>

<!DOCTYPE html>
<html lang="en">
<?php
	$excludeUndocumentedFunctions = true;

	function getData()
	{
		$jsonData = \JsonMachine\JsonMachine::fromFile('documentation.json');
		$allData = array();
		
		foreach($jsonData as $type => $types)
		{	
			foreach($types as $typeName => $typeNames)
			{
				foreach($typeNames as $n => $data)
				{
					if((isset($data["param"]["desc"]) && $data["param"]["desc"] != "" || !$GLOBALS["excludeUndocumentedFunctions"]) && !isset($data["param"]["local"])) // exclude unset functions or @local functions
					{
						if (!isset($data["param"]["realm"]))
						{
							$data["param"]["realm"] = "shared";
						}
						
						$data["type"] = $type;
						$data["typeName"] = $typeName;
						
						array_push($allData, $data);
					}
				}
			}
		}
		
		return $allData;
	}
	
	function getTypes()
	{
		$jsonData = \JsonMachine\JsonMachine::fromFile('documentation.json');
		$ts = array();
		
		foreach($jsonData as $type => $types)
		{
			foreach($types as $n => $typeNames)
			{
				$added = false;
				
				foreach($typeNames as $n2 => $data)
				{
					if(count($data) > 0) // check if empty
					{
						array_push($ts, $type);
						
						$added = true;
						
						break;
					}
				}
				
				if ($added)
				{
					break;
				}
			}
		}
		
		return $ts;
	}
	
	function getTypeNames($typ)
	{
		$jsonData = \JsonMachine\JsonMachine::fromFile('documentation.json');
		$tns = array();
		
		foreach($jsonData as $type => $types)
		{
			foreach($types as $typeName => $typeNames)
			{
				if ($type == $typ)
				{
					foreach($typeNames as $n2 => $data)
					{
						if(count($data) > 0) // check if empty
						{
							array_push($tns, $typeName);
							
							break;
						}
					}
				}
			}
		}
		
		return $tns;
	}

	function getFunctions($typ, $typName)
	{
		$jsonData = \JsonMachine\JsonMachine::fromFile('documentation.json');
		$fns = array();
		
		foreach($jsonData as $type => $types)
		{
			if($type == $typ)
			{
				foreach($types as $typeName => $typeNames)
				{
					if($typeName == $typName)
					{
						foreach($typeNames as $n => $data)
						{
							if((isset($data["param"]["desc"]) && $data["param"]["desc"] != "" || !$GLOBALS["excludeUndocumentedFunctions"]) && !isset($data["param"]["local"])) // exclude unset functions or @local functions
							{
								if(!isset($data["param"]["realm"]))
								{
									$data["param"]["realm"] = "shared";
								}
								
								array_push($fns, array(
									"name" => $data["name"], 
									"realm" => $data["param"]["realm"],
									"type" => $type,
									"typeName" => $typeName
								));
							}
						}
					}
				}
			}
		}
		
		return $fns;
	}
	
	function replaceType($type)
	{
		if ($type == "?")
		{
			return $type;
		}
		
		$arr = explode("|", $type);
		
		for ($i = 0; $i < count($arr); $i++)
		{
			if ($i != 0)
			{
				$ret .= " / ";
			}
			
			$ret .= '<a href="https://wiki.garrysmod.com/page/' . str_replace(':', '/', str_replace('.', '/', $arr[$i])) . '">' . $arr[$i] . '</a>';
		}
		
		return $ret;
	}
	
	function searchReplace($text)
	{
		preg_match_all('/@{([\.*:*\w+]*)}/', $text, $arr, PREG_OFFSET_CAPTURE);
		
		for ($i = 0; $i < count($arr[0]); $i++)
		{	
			$text = str_replace($arr[0][$i][0], replaceType($arr[1][$i][0]), $text);
		}
		
		return $text;
	}
	
	function getPrefix($data)
	{
		$pre = "";
		
		if(isset($data["typeName"]) && $data["typeName"] != "")
		{
			if(isset($data["type"]))
			{
				$pre = $data["typeName"];
				
				if($data["type"] == "module")
				{
					$pre .= ".";
				}
				elseif($data["type"] == "type")
				{
					$pre .= ":";
				}
			}
		}
		
		return $pre;
	}
?>
<head>
	<meta charset="utf-8">
	<meta name="viewport" content="width=device-width, initial-scale=1">

	<link rel="stylesheet" type="text/css" href="stylish.css">

	<script src="https://ajax.googleapis.com/ajax/libs/jquery/1.12.0/jquery.min.js"></script>
	<script src="script.js"></script>

	<link href="https://fonts.googleapis.com/css?family=Dosis" rel="stylesheet">
</head>
<body>
	<div id="navlist">
		<?php
			$types = getTypes();
			
			foreach($types as $n => $type)
			{
				$typeNamesList = getTypeNames($type);
				
				foreach($typeNamesList as $n2 => $typeName)
				{
					echo '<span class="navlist-element parent wrapper" style="font-size: 14px;">' . $type . ' -> ' . $typeName . '</span><br />';
					
					$funcs = getFunctions($type, $typeName);
				
					foreach($funcs as $n3 => $data)
					{
						//preprocess name
						$name = $data["name"];
						$name_parts = explode('.', $name);
						$name_type_found = strpos($name, ":") !== false;
						$name_print = '<a href="?func=' . getPrefix($data) . $name . '"><span class="navlist-element wrapper">';

						for($i = 0; $i < count($name_parts) - 1; $i++) 
						{
							$name_print .= '<span class="navlist-element prefix type-module">' . $name_parts[$i];
							
							if($i < count($name_parts) - 1) {
								$name_print .= '.';
							}
							
							$name_print .= '</span>';
						}
						
						if($name_type_found)
						{
							$name_type = explode(':', $name);
							$name_print .= '<span class="navlist-element prefix hook">' . $name_type[0] . ':</span>';
						}
						
						if($name_type_found)
						{
							$text = end($name_type);
						}
						else 
						{
							$text = end($name_parts);
						}
						
						$name_print .= '<span class="navlist-element ' . strtolower($data["realm"]) . '">' . $text . '</span></span>';
						$name_print .= '</a>';

						echo $name_print;
					}
				}
			}
		?>
	</div>
	<div id="code-container">
		<div id="code">
			<?php
				// TODO receive following data from server to improve performance
				$funcData = getData();
				
				if(isset($_GET) && isset($_GET["func"]))
				{
					foreach($funcData as $n => $data)
					{	
						if(getPrefix($data) . $data["name"] == $_GET["func"])
						{
							$requestedFunction = $data;
							
							break;
						}
					}
				}

				if(isset($requestedFunction))
				{
					$args = "";
					
					if(isset($requestedFunction["param"]["param"]))
					{
						foreach($requestedFunction["param"]["param"] as $n => $arg)
						{	
							$arr = explode(" ", $arg);
							
							$type = replaceType($arr[0]);
							$str = $type . " " . $arr[1];
							
							$args .= $str . ', ';
						}
						
						$args = substr($args, 0, -2);
						$args = str_replace("_UDF_PRM_", "?", $args);
					}
					
					echo '<span class="code-funcname ' . strtolower($requestedFunction["param"]["realm"]) . '">' . getPrefix($requestedFunction) . $requestedFunction["name"] . '</span><span class="code-funcargs">( ' . $args . ' )</span><br>';

					if(isset($requestedFunction["param"]["desc"]))
					{
						echo '<p><span class="code-desc">Desc: ' . searchReplace($requestedFunction["param"]["desc"]) . '</span></p>';
					}

					echo '<p><span class="code-note">Note: ' . (isset($requestedFunction["param"]["note"]) ? $requestedFunction["param"]["note"] : "None" ) . '</span></p>';
					echo '<p><span class="code-source">Source: <a href="https://github.com/TTT-2/TTT2/tree/master/' . $requestedFunction["path"] . '#L' . $requestedFunction["line"] . '">' . $requestedFunction["path"] . ':' . $requestedFunction["line"] . '</a></span></p>';
				}
			?>
		</div>
	</div>
</body>
</html>
