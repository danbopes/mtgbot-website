function adminicaForms() {
	// Form inputs

	$("fieldset > div > input[type=text]").addClass("text");
	$("fieldset > div > input[type=email]").addClass("text");
	$("fieldset > div > input[type=number]").addClass("text");
	$("fieldset > div > input[type=password]").addClass("text");
	$("fieldset > div > textarea").addClass("textarea");
	$("fieldset > div > input[type=checkbox]").addClass("checkbox");
	$("fieldset > div > input[type=radio]").addClass("radio");
	$("fieldset > div > input[type=checkbox].indeterminate").prop("indeterminate", true);


	// Form Validation

	if($.fn.validate){
		$(".validate_form").validate();
	}

	// Dismiss alert box

	$(".alert.dismissible").on("click",function(){
		$(this).animate({opacity:0},'slow',function(){
			$(this).slideUp();
		});
	});


	// Textxarea Autogrow

	if($.fn.autoGrow){
		$('textarea.autogrow').autoGrow();
	}


	// Input Datepicker Config
	if($.fn.datepicker){
		$( ".datepicker" ).datepicker({
			dateFormat: 'd M yy',
			showOn:'focus'
		});
	}

	// input Slider	Config

	if($.fn.slider){

		function slideMarkers(event,ui){
			var totalLabels = $(this).children().children().size();
			$(this).children("ol.slider_labels").children("li").css({
				"margin-right":(100/(totalLabels-1))+"%"
			});
		}

		$( ".slider" ).slider({
			min: "0",
			max: "100",
			range: "min",
			slide: function( event, ui ) {
				$("#slider_value").text( ui.value );
			},
			create: slideMarkers

		}); // creates a simple slider with default settings

	}


	// input Range Slider Config

	if($.fn.slider){
		$( ".slider_range" ).slider({
			range: true, // creates a range slider
			min: 0,
			max: 500,
			values: [ 75, 300 ],
			slide: function( event, ui ) {
				$( "#amount" ).val( "$" + ui.values[ 0 ] + " - $" + ui.values[ 1 ] );
			}
		});

		$( "#amount" ).val( "$" + $( "#slider_range" ).slider( "values", 0 ) +
			" - $" + $( "#slider_range" ).slider( "values", 1 ) );

		$(".slider_vertical > span").each(function() {
			var value = parseInt($(this).text());
			$(this).empty().slider({
				value: value,
				range: "min",
				animate: true,
				orientation: "vertical"
			});
		});


		// Unlock Slider

		function unlockMsg(){
			var unlockMessage =	$(this).attr("title");

			$(this).append('<div class="unlock_message">'+unlockMessage+'</div>');
		}

		function fixBounds(){
			var value = $(this).slider("value");
			var fixMargin = (value/100*-30);

			$(this).find(".ui-slider-handle").css("margin-left",fixMargin+"px");
		}

		function unlocked(e,ui){

			if($(this).slider("value") > 95){

				$(this).siblings("button, input").trigger("click");

				$(this).find(".ui-slider-handle").delay(500).animate({
						left: '0%',
						"margin-left": 0
						}, 350 );
				$(this).find(".ui-slider-range").delay(500).animate({width:0}, function(){
					$(this).slider("value", 0);
				});
			}
			else{
				$(this).find(".ui-slider-handle").animate({
					left: '0%',
					"margin-left": 0
					} );
				$(this).find(".ui-slider-range").animate({width:0}, function(){
					$(this).slider("value", 0);
				});
			}

		}

		$(".slider_unlock").slider({
			value: "0",
			range: "min",
			animate: true,
			stop: unlocked,
			slide: fixBounds,
			change: fixBounds,
			create: unlockMsg
		});
	}


	//Progress Bar Config

	if($.fn.progressbar){
		$( ".progressbar" ).progressbar({
			value: 75
		});
	}


	// jQuery UI buttons

	if($.fn.buttonset){
		$(".jqui_checkbox").buttonset();

		$(".jqui_radios").buttonset();
		$(".jqui_radios > label").on("click",function(){
			$(this).siblings().removeClass("ui-state-active");
		}); // jQuery UI radio buttonset fix
	}


    // Uniform Form Styles

	if($.fn.uniform){

	    setTimeout('$(".uniform input, .uniform, .uniform a, .time_picker_holder select").uniform();',10);
	}



	// jQuery Knob

	if($.fn.knob){
		$(".knob").knob();
	}



	// Drag and Drop Select

	if($.fn.multiselect){
		$(".multisorter").multiselect();
	}


	// Time Picker

	if($.fn.timepicker){
		$(".time_picker").timepicker();
	}


	// Colour Picker

	if($.fn.ColorPicker){

		$('#colorpicker_inline').ColorPicker({flat: true});

		$('#colorpicker_popup').ColorPicker({
			onSubmit: function(hsb, hex, rgb, el) {
				$(el).val(hex);
				$(el).ColorPickerHide();
			},
			onBeforeShow: function () {
				$(this).ColorPickerSetColor(this.value);
			}
		})
		.on('keyup', function(){
			$(this).ColorPickerSetColor(this.value);
		});
	}


	// Star Ratings

	if($.fn.stars){
		$("#star_rating").stars({inputType: "select"});
	}


	// Tooltip
	if($.fn.tipTip){
		$(".tooltip").tipTip({
			defaultPosition: "top",
			maxWidth: "auto",
			edgeOffset: 0
		});
	}


	// Autocomplete

	var autoCompleteList = [
			"ActionScript",
			"AppleScript",
			"Asp",
			"BASIC",
			"C",
			"C++",
			"Clojure",
			"COBOL",
			"ColdFusion",
			"Erlang",
			"Fortran",
			"Groovy",
			"Haskell",
			"Java",
			"JavaScript",
			"Lisp",
			"Perl",
			"PHP",
			"Python",
			"Ruby",
			"Scala",
			"Scheme"
		];
		$(".autocomplete").autocomplete({
			source: autoCompleteList
		});


	// Tag It Input

	if($.fn.tagit){
		setTimeout("$('.tagit').tagit();", 3000);
	}


	// Dialog Config

	if($.fn.dialog){
		$(".dialog_content").dialog({
			autoOpen: false,
			resizable: false,
			show: "fade",
			hide: "fade",
			modal: true,
			width: "500",
        	show:{effect: "fade", duration: 500},
        	hide:{effect: "fade", duration: 500},
        	create: function(){
        		$('.dialog_content.no_dialog_titlebar').dialog('option', 'dialogClass', 'no_dialog_titlebar');
        	},
        	open: function(){
        		setTimeout(columnHeight, 100);
        	}
		});

		$(".dialog_button").on("click", function() {
			var theDialog = $(this).attr('data-dialog');
			$("#"+theDialog).dialog( "open" ); // the #dialog element activates the modal box specified above
			return false;
		});

		$(".close_dialog").on("click", function() {
			$(".dialog_content").dialog( "close" ); // the #dialog element activates the modal box specified above
			return false;
		});

		$(".link_button").on("click", function(){
			var x = $(this).attr("data-link");

			window.location.href = x;

			return false;
		});

		$(".dialog_content.very_narrow").dialog( "option" , "width" , 300 );
		$(".dialog_content.narrow").dialog( "option" , "width" , 450 );
		$(".dialog_content.wide").dialog( "option" , "width" , 650 );
		$(".dialog_content.medium_height").dialog( "option" , "height" , 315 );

		$(".dialog_content.no_modal").dialog( "option" , "modal" , false );
		$(".dialog_content.no_modal").dialog( "option" , "draggable" , false );

		$(".ui-widget-overlay").on("click", function(){
			$(".dialog_content").dialog( "close" );
			return false;
		});

	}


	// Bounce Slider

	if($.fn.slider){

		function dialogClose(e,ui){
			if($(this).slider("value") > 95){
				$("#dialog_content_1").dialog("close");
				$(this).find(".ui-slider-handle").animate({left: 0}, 350 );
				$(this).find(".ui-slider-range").animate({width:0});
			}
			else{
				$(this).find(".ui-slider-handle").animate({left: 0}, 350 );
				$(this).find(".ui-slider-range").animate({width:0});
			}
		}

		$("#slider_close_dialog").slider({
			value: "0",
			range: "min",
			animate: true,
			stop: dialogClose
		});
	}


	// Select2 select box

	if($.fn.select2){
	    $(".select2").select2({
		    allowClear: true,
		    minimumResultsForSearch: 10
	    });
	}
}



