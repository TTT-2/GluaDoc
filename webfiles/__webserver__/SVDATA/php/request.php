<?php

require_once('lib/StreamBytes.php');
require_once('lib/Parser.php');
require_once('lib/Lexer.php');
require_once('lib/JsonMachine.php');

// GLOBALS
$file = \JsonMachine\JsonMachine::fromFile('../data/documentation.json');
$notype = 'notype';

// HANDLING
$data = json_decode($_POST['data']);
$action = $data->action;

if ($action == 'request' && $data->type == 'types')
    request_type();

if ($action == 'request_elem_list')
    request_elem_list($data->args);

if ($action == 'request_elem_code')
    request_elem_code($data->args);


// FUNCTIONS
function request_type() {
    global $file, $notype;

    $types = [];
    foreach ($file as $new_type => $value) {
        if ($new_type == '')
            $new_type = $notype;
        $types[$new_type] = [];
        foreach ($value as $new_sub_type => $sub_value) {
            if ($new_sub_type == '')
                $new_sub_type = $notype;
            $types[$new_type][$new_sub_type] = [];
        }
    }
    echo json_encode($types);
}

function request_elem_list($args) {
    global $file, $notype;

    if ($args[0] == $notype)
        $args[0] = '';

    if ($args[1] == $notype)
        $args[1] = '';

    $arr = [];
    foreach ($file as $new_type => $value) {
        if ($new_type != $args[0])
            continue;

        foreach ($value as $new_sub_type => $sub_value) {
            if ($new_sub_type != $args[1])
                continue;

            foreach ($sub_value as $key => $obj) {
                $arr[$obj['name']] = [];
                $arr[$obj['name']]['name'] = [$obj['name']];
                if (isset($obj['param']['realm']))
                    $arr[$obj['name']]['param']['realm'] = $obj['param']['realm'];
                else
                    $arr[$obj['name']]['param']['realm'] = 'shared';
            }
        }
    }

    //print_r($arr);
    echo json_encode($arr);
}

function request_elem_code($args) {
    global $file, $notype;

    if ($args[0] == $notype)
        $args[0] = '';

    if ($args[1] == $notype)
        $args[1] = '';

    foreach ($file as $new_type => $value) {
        if ($new_type != $args[0])
            continue;

        foreach ($value as $new_sub_type => $sub_value) {
            if ($new_sub_type != $args[1])
                continue;

            foreach ($sub_value as $key => $obj) {
                if ($obj['name'] == $args[2]) {
                    echo json_encode($obj);
                    break;
                }
            }
        }
    }
}



?>