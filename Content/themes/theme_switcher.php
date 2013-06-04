<?php
/* PHP CSS style switcher v1.0
 * (c) 2011 Web factory Ltd
 * http://www.webfactoryltd.com/
*/

    // default CSS file
    $defaultStyle = '';

    // after style change redirect to this URL
    $redirectUrl = '';

    // cookie expire time; session, hour, day, week, month, year, or number of seconds
    $expireTime = 'session';

    // controls weather the CSS file is gziped on output
    $gzipOutput = true;


/**
 *
 * !!! DO NOT EDIT BELOW THIS LINE !!!
 * !!! DO NOT EDIT BELOW THIS LINE !!!
 *
 */
    // keep unwanted output from happening
    ob_start();
    $file = str_replace(array('.'), array('_'), basename(strtolower(__FILE__)));

    // check if the defined file is a CSS file and readable
    function check_file($file) {
        // check for possible hack attempt,only .css filea are allowed
        if (strtolower(substr($file, -4)) != '.css') {
            if (isset($_GET['default'])) {
                die('The "default" parameter has to be a CSS file and end with ".css".');
            } else {
                die('The "style" parameter has to be a CSS file and end with ".css".');
            }
        }

        // check if the file is readable
        if (!is_readable($file)) {
            header('HTTP/1.0 404 Not Found');
            die("Can't open \"{$file}\". Please check the file name and path.");
        }

        return true;
    }


    // switching styles
    if( isset($_GET['style']) && !empty($_GET['style'])) {
        // determine cookie lifetime
        $expireTime = strtolower($expireTime);
        if ($expireTime == 'session') {
            $expire = 0;
        } elseif ($expireTime == 'hour') {
            $expire = time() + 60*60;
        } elseif ($expireTime == 'day') {
            $expire = time() + 60*60*24;
        } elseif ($expireTime == 'week') {
            $expire = time() + 60*60*24*7;
        } elseif ($expireTime == 'month') {
            $expire = time() + 60*60*24*30;
        } elseif ($expireTime == 'year') {
            $expire = time() + 60*60*24*365;
        } elseif (is_numeric($expireTime)) {
            $expire = $expireTime;
        } else {
            $expire = 0;
        }

        if (strtolower($_GET['style']) != 'multiple') {
            // check single style file
            check_file(trim($_GET['style']));
            // save the style filename into cookie
            if (!setcookie($file . '-style', $_GET['style'], $expire, '/')) {
                die("Unable to set cookie '{$file}-style'.");
            };
        } else {
            // handle multiple style changes
            foreach($_GET as $param => $value) {
                if (substr(strtolower($param), -4) == '_php' || substr(strtolower($param), -4) == '.php') {
                    check_file(trim($value));
                    $file = str_replace(array('.'), array('_'), basename(strtolower($param)));
                    if (!setcookie($file . '-style', $value, $expire, '/')) {
                      die("Unable to set cookie '{$file}-style'.");
                    };

                }
            }
        }

        // redirect the user to a specified URL or the URL he came from
        if (isset($_GET['redirect']) && !empty($_GET['redirect'])) {
            header('location: ' . $_GET['redirect']);
        } elseif (isset($redirectUrl) && !empty($redirectUrl)) {
            header('location: ' . $redirectUrl);
        } elseif (isset($_SERVER['HTTP_REFERER']) && !empty($_SERVER['HTTP_REFERER'])) {
            header('location: ' . $_SERVER['HTTP_REFERER']);
        } else {
            // no fererer is set, let's try to guess the correct URL
            if (strpos(strtolower($_GET['style']), 'css/') !== false) {
                header('location: ../');
            } else {
                header('location: /');
            }
        }
        die();
    }
    //switching styles


    // displaying the style
    // prefer the one set in cookie
    if (isset($_COOKIE[$file . '-style']) && !empty($_COOKIE[$file . '-style'])) {
        check_file($_COOKIE[$file . '-style']);
        $filename = $_COOKIE[$file . '-style'];
    } elseif (isset($_GET['default']) && !empty($_GET['default'])) {
        // try the one defined via GET parameter
        check_file(trim($_GET['default']));
        $filename = $_GET['default'];
    } elseif (isset($defaultStyle) && !empty($defaultStyle)) {
        // try the one defined via $defaultStyle variable
        check_file(trim($defaultStyle));
        $filename = $defaultStyle;
    } else {
        die('Please define a default CSS file with "default" GET parameter or via the PHP variable; or set the current style with the "style" GET parameter.');
    }


    // output the content of the CSS file
    ob_end_clean();
    if ($gzipOutput) {
        ob_start('ob_gzhandler');
    }
    // prevent caching to force fresh CSS
    header('Cache-Control: no-cache, must-revalidate');
    header('Expires: Sat, 26 Jul 1997 05:00:00 GMT');
    header('Content-Type: text/css');
    readfile(trim($filename));
?>