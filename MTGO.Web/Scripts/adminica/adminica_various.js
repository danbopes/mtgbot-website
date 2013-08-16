function adminicaVarious() {
    return;
	// Contacts Page

	//This configures the iPhone style Contacts display)
	if($.fn.sliderNav){

 		$('#slider_list').sliderNav({height:'402'});

		$('#slider_list ul ul li a').on('click', function(){
			var y =	$(this).find('span').text();
			var x =	$(this).html().replace('<span>'+y+'</span>','');
			var numRand = Math.floor(Math.random()*11)
			$('#contactName').text(x);
			$('#contactEmail').text(y);
			$('#contactImage').attr("src","images/content/profiles/mangatar-"+numRand+".png");
		});
	}



	// Tiny Editor

	if($(".tinyeditor").length > 0){
		new TINY.editor.edit('editor',{
			id:'tiny_input',
			height:200,
			cssclass:'te',
			controlclass:'tecontrol',
			rowclass:'teheader',
			dividerclass:'tedivider',
			controls:['bold','italic','underline','strikethrough','|','subscript','superscript','|',
					  'orderedlist','unorderedlist','|','outdent','indent','|','leftalign',
					  'centeralign','rightalign','blockjustify','|','unformat','|','undo','redo','n','image','hr','link','unlink','|','cut','copy','paste','print','|','font','size','style'],
			footer:false,
			fonts:['Arial','Verdana','Georgia','Trebuchet MS'],
			xhtml:true,
			bodyid:'editor',
			footerclass:'tefooter',
			toggle:{text:'source',activetext:'wysiwyg',cssclass:'toggler'},
			resize:{cssclass:'resize'}
		});

		new TINY.editor.edit('editor2',{
			id:'tiny_input2',
			height:200,
			cssclass:'te',
			controlclass:'tecontrol',
			rowclass:'teheader',
			dividerclass:'tedivider',
			controls:['bold','italic','underline','strikethrough','|','subscript','superscript','|',
					  'orderedlist','unorderedlist','|','outdent','indent','|','leftalign',
					  'centeralign','rightalign','blockjustify','|','unformat','|','undo','redo','n','image','hr','link','unlink','|','cut','copy','paste','print','|','font','size','style'],
			footer:false,
			fonts:['Arial','Verdana','Georgia','Trebuchet MS'],
			xhtml:true,
			bodyid:'editor',
			footerclass:'tefooter',
			toggle:{text:'source',activetext:'wysiwyg',cssclass:'toggler'},
			resize:{cssclass:'resize'}
		});

		$(".teheader select").uniform();
	}



	// ElFinder

	if($.fn.elfinder){
			var f = $('#finder').elfinder({
				url : 'scripts/elfinder/connectors/php/connector.php',
				places: '',
				toolbar : [
					['back', 'reload'],['mkdir','copy','paste'],['remove','rename','info'],['icons','list']
				],

				// dialog : {
				// 	title : 'File manager',
				// 	height : 500
				// }

				// Callback example
				//editorCallback : function(url) {
				//	if (window.console && window.console.log) {
				//		window.console.log(url);
				//	} else {
				//		alert(url);
				//	}
				//},
				//closeOnEditorCallback : true
			});

			// window.console.log(f)
			$('#close,#open,#dock,#undock').click(function() {
				$('#finder').elfinder($(this).attr('id'));
			});

	}


	// Toggle Form lines

	$("#toggle_lines").click(function(){
		$(".main_container").toggleClass("no_lines");
	});


	// Page select box in dialog welcome

	$("#dialog_welcome").dialog({
		open: function(){
			if($("#pagesSelect .main").length<1){
				$('#mobile_nav .main').clone('copy').appendTo('#pagesSelect');
			}
		    $("#pagesSelect select").change(function(){
		    	theLink = $(this).val();
		    	window.location = theLink;
		    });


		if($.fn.select2){
	    	$(".select2").addClass("full_width").select2();
	    }
		}
	});
}