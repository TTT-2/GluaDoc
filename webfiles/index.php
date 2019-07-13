<?php
	require_once __DIR__ . '/bootstrap.php';
?>

<!DOCTYPE html>
<html lang="en">
<?php
	$excludeUndocumentedFunctions = false;

	function getData()
	{
		$jsonData = \JsonMachine\JsonMachine::fromFile('documentation.json');
		$allData = array();
		
		foreach($jsonData as $n => $category)
		{
			foreach($category as $n2 => $data)
			{
				if((count($data["param"]) > 0 || !$GLOBALS["excludeUndocumentedFunctions"]) && !isset($data["param"]["local"])) // exclude unset functions or @local functions
				{
					if (!isset($data["param"]["realm"]))
					{
						$data["param"]["realm"] = "shared";
					}
					
					array_push($allData, $data);
				}
			}
		}
		
		return $allData;
	}
	
	function getCategories()
	{
		$jsonData = \JsonMachine\JsonMachine::fromFile('documentation.json');
		$cats = array();
		
		foreach($jsonData as $name => $category)
		{
			foreach($category as $n => $data)
			{
				if(count($data) > 0)
				{
					array_push($cats, $name);
					
					break;
				}
			}
		}
		
		return $cats;
	}

	function getFunctions($cat)
	{
		$jsonData = \JsonMachine\JsonMachine::fromFile('documentation.json');
		$fns = array();
		
		foreach($jsonData as $name => $category)
		{
			if($name == $cat || !isset($cat))
			{
				foreach($category as $n => $data)
				{
					if((count($data["param"]) > 0 || !$GLOBALS["excludeUndocumentedFunctions"]) && !isset($data["param"]["local"])) // exclude unset functions or @local functions
					{
						if(!isset($data["param"]["realm"]))
						{
							$data["param"]["realm"] = "shared";
						}
						
						array_push($fns, array(
							"name" => $data["name"], 
							"realm" => $data["param"]["realm"]
						));
					}
				}
			}
		}
		
		return $fns;
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
			$cats = getCategories();
			
			foreach($cats as $n => $cat)
			{
				echo '<span class="navlist-element parent wrapper" style="font-size: 14px;">' . $cat . '</span><br />';
				
				$funcs = getFunctions($cat);
			
				foreach($funcs as $n2 => $data)
				{
					//preprocess name
					$name = $data["name"];
					$name_parts = explode('.', $name);
					$name_type = explode(':', $name);
					$name_print = '<a href="?func=' . $name . '"><span class="navlist-element wrapper">';

					for($i = 0; $i < count($name_parts) - 1; $i++) 
					{
						$name_print .= '<span class="navlist-element prefix type-module">' . $name_parts[$i];
						
						if ($i < count($name_parts) - 1) {
							$name_print .= '.';
						}
						
						$name_print .= '</span>';
					}
					
					if(count($name_type) > 1)
					{
						$name_print .= '<span class="navlist-element prefix hook">' . $name_type[1] . ':</span>';
						$isType = true;
					}
					
					$name_print .= '<span class="navlist-element ' . strtolower($data["realm"]) . '">' . (isset($isType) ? end($name_type) : end($name_parts)) . '</span></span>';
					$name_print .= '</a>';

					echo $name_print;
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
						if($data["name"] == $_GET["func"])
						{
							$requestedFunction = $data;
							
							break;
						}
					}
				}

				if(isset($requestedFunction))
				{
					echo '<span class="code-funcname ' . strtolower($requestedFunction["param"]["realm"]) . '">' . $requestedFunction["name"] . '</span><span class="code-funcargs">( ' . (isset($requestedFunction["param"]["args"]) ? $requestedFunction["param"]["args"] : "" ) . ' )</span><br>';

					if(isset($funcs[$i]["param"]["desc"]))
					{
						echo '<span class="code-desc">Desc: ' . $requestedFunction["param"]["desc"] . '</span><br>';
					}

					echo '<span class="code-note">Note: ' . (isset($requestedFunction["param"]["note"]) ? $requestedFunction["param"]["note"] : "None" ) . '</span><br />';
					echo '<span class="code-source">Source: <a href="https://github.com/TTT-2/TTT2/tree/master/' . $requestedFunction["path"] . '#L' . $requestedFunction["line"] . '">' . $requestedFunction["path"] . ':' . $requestedFunction["line"] . '</a></span>';
				}
			?>
		</div>
	</div>
</body>
</html>
