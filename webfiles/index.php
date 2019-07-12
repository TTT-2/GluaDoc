<!DOCTYPE html>
<html lang="en">
<?php
		$f = file_get_contents("documentation.json");
		$funcs = json_decode($f, true);

		//strip undocumented
		foreach($funcs as $n => $data)
		{
				$funcs[$n] = count($data["param"]) > 0 ? $funcs[$n] : false;

				if(!$funcs[$n] || isset($funcs[$n]["param"]["local"])) // exclude unset functions or @local functions
				{
						unset($funcs[$n]);
				}
				else
				{
						if (!isset($data["param"]["realm"]))
						{
								$funcs[$n]["param"]["realm"] = "shared";
						}
				}
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
						foreach($funcs as $n => $data)
						{
								echo
								'
										<a href="?func=' . $data["name"] . '">
												<span class="navlist-element ' . strtolower($data["param"]["realm"]) . '">
														' . $data["name"] . '
												</span>
										</a>
								';
						}
				?>
		</div>
		<div id="code-container">
				<div id="code">
						<?php
								if(isset($_GET) && isset($_GET["func"]))
								{
										foreach($funcs as $n => $data)
										{
												if($data["name"] == $_GET["func"])
												{
														$i = $n;

														break;
												}
										}
								}

								if(isset($i))
								{
										echo '<span class="code-funcname ' . strtolower($funcs[$i]["param"]["realm"]) . '">' . $funcs[$i]["name"] . '</span><span class="code-funcargs">( ' . (isset($funcs[$i]["param"]["args"]) ? $funcs[$i]["param"]["args"] : "" ) . ' )</span><br>';

										if(isset($funcs[$i]["param"]["desc"]))
										{
												echo '<span class="code-desc">DESC: ' . $funcs[$i]["param"]["desc"] . '</span><br>';
										}

										echo '<span class="code-note">NOTE: ' . (isset($funcs[$i]["param"]["note"]) ? $funcs[$i]["param"]["note"] : "None" ) . '</span>';
								}
						?>
				</div>
		</div>
</body>
</html>
