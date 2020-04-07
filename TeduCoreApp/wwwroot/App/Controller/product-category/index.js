var productCategoryController = function () {
    this.initialize = function () {
        registerEvents();
        loadData();
    }

    function registerEvents() {
        $('#btnCreate').off('click').on('click',
            function() {
                $('#model-add-edit').modal('show');
            });

        $('body').on('click',
            '#btnEdit',
            function(e) {
                e.preventDefault();
                var that = $('#hidIdM').val();
                $.ajax({
                    type:'GET',
                    url: '/Admin/ProductCategory/GetById',
                    data: { id: that },
                    dataType: 'JSON',
                    beforeSend: function() {
                        tedu.startLoading();
                    },
                    success: function(response) {
                        var data = response;
                        $('#hidIdM').val(data.Id);
                        $('#txtNameM').val(data.Name);
                        initTreeDropDownCategory(data.CategoryId);
                        $('#txtDescM').val(data.Description);
                        $('#txtImageM').val(data.ThumbnailImage);

                        $('#txtSeoKeywordM').val(data.SeoKeyWord);
                        $('#txtSeoDescriptionM').val(data.SeoDescription);
                        $('#txtSeoPageTitleM').val(data.SeoPageTitle);
                        $('#txtSeoAliasM').val(data.SeoAlias);

                        $('#ckStatusM').prop('checked', data.Status == 0);
                        $('#ckShowHomeM').prop('checked', data.HomeFlag);
                        $('#txtOrderM').val(data.Order);
                        $('#txtHomeOrderM').val(data.HomeOder);

                        $('#model-add-edit').modal('show');
                        tedu.stopLoading();
                    },
                    error: function(status) {
                        tedu.notify("Có lỗi xảy ra", "error");
                        tedu.stopLoading();
                    }
                });
            });

        $('body').on('click', '#btnDelete', function (e) {
            e.preventDefault();
            var that = $('#hidIdM').val();
            tedu.confirm('Are you sure to delete?', function () {
                $.ajax({
                    type: "POST",
                    url: "/Admin/ProductCategory/Delete",
                    data: { id: that },
                    dataType: "json",
                    beforeSend: function () {
                        tedu.startLoading();
                    },
                    success: function (response) {
                        tedu.notify('Deleted success', 'success');
                        tedu.stopLoading();
                        loadData();
                    },
                    error: function (status) {
                        tedu.notify('Has an error in deleting progress', 'error');
                        tedu.stopLoading();
                    }
                });
            });
        });
        $('#frmMaintainance').validate({
            errorClass: 'red',
            ignore: [],
            lang: 'en',
            rules: {
                txtNameM: { required: true },
                txtOrderM: { required: true },
                txtHomeOrderM: {required: true}
            }
        });

        $('#btnSave').on('click', function(e, item) {
            e.preventDefault();
            initTreeDropDownCategory();
            if ($('#frmMaintainance').valid()) {
                var id = $('#hidIdM').val();
                var name = $('#txtNameM').val();
                var parentId = $('#ddlCategoryIdM').val();
                var description = $('#txtDescM').val();

                var image = $('#txtImageM').val();
                var order = $('#txtOrderM').val();
                var homeOrder = $('#txtHomeOrderM').val();

                var seoKeyword = $('#txtSeoKeywordM').val();
                var seoMetaDescriptions = $('#txtSeoDescriptionM').val();
                var seoPageTitle = $('#txtSeoPageTitleM').val();
                var seoAlias = $('#txtSeoAliasM').val();
                var status = $('#ckStatusM').prop('checked') == true ? 0 : 1;
                var showHome = $('#ckShowHomeM').prop('checked');

                $.ajax({
                    url: '/Admin/ProductCategory/SaveEntity',
                    type: 'POST',
                    dataType: 'JSON',
                    data: {
                        Id: id,
                        Name: name,
                        ParentId: parentId,
                        Description: description,
                        Image: image,
                        HomeOrder: homeOrder,
                        HomeFlag: showHome,
                        SortOrder: order,
                        SeoPageTitle: seoPageTitle,
                        SeoAlias: seoAlias,
                        SeoKeywords: seoKeyword,
                        Status: status,
                        SeoDescription: seoMetaDescriptions
                    },
                    beforeSend: function () {
                        tedu.startLoading();
                    },
                    success: function (response) {
                        tedu.notify("Update Success", "success");
                        $('#model-add-edit').modal('hide');

                        resetFormMaintainance();
                        tedu.stopLoading();
                        loadData(true);
                    },
                    error: function (e) {
                        tedu.notify("Has an error", 'error');
                        tedu.stopLoading();
                    }
                });
            }
            return false;   
        });
    }

    function resetFormMaintainance() {
        $('#hidIdM').val();
        $('#txtNameM').val();
        $('#ddlCategoryIdM').val();
        $('#txtDescM').val();
        initTreeDropDownCategory(' ');

        $('#txtImageM').val();
        $('#txtOrderM').val();
        $('#txtHomeOrderM').val();

        $('#txtSeoKeywordM').val();
        $('#txtSeoDescriptionM').val();
        $('#txtSeoPageTitleM').val();
        $('#txtSeoAliasM').val();
        $('#ckStatusM').prop('checked', true);
        $('#ckShowHomeM').prop('checked',false);
    } 

    function initTreeDropDownCategory(selectedId) {
        $.ajax({
            url: '/Admin/ProductCategory/GetAll',
            type: 'GET',
            dataType: 'JSON',
            async: false,
            data: {},
            success: function(response) {
                var data = [];
                $.each(response,
                    function (i, items) {
                        data.push({
                            id: items.Id,
                            text: items.NAME,
                            parentId: items.ParentId,
                            sortOrder: items.SortOrder
                        });
                    });
                var arr = tedu.unflattern(data);
                $('#ddlCategoryIdM').combotree({
                    data: arr
                });
                if (selectedId != undefined) {
                    $('#ddlCategoryIdM').combotree('setValue', selectedId);
                }
            }
        });
    }

    function loadData() {
        $.ajax({
            url: '/admin/ProductCategory/GetAll',
            dataType: 'JSON',
            Data: {},
            success: function (response) {
                var data = [];
                $.each(response,
                    function (i, item) {
                        data.push({
                            id: item.Id,
                            text: item.Name,
                            parentId: item.ParentId,
                            sortOrder: item.SortOrder
                        });
                    });
                // sắp xếp các danh mục lại với nhau
                var treeArr = tedu.unflattern(data);

                treeArr.sort(function (a, b) {
                    return a.sortOrder - b.sortOrder;
                });
                //var $tree = $('#treeProductCategory');

                $('#treeProductCategory').tree({
                    data: treeArr,
                    dnd: true,
                    onContextMenu: function (e, node) {
                        e.preventDefault();
                        //select the node
                        $('#hidIdM').val(node.id);
                        //display context menu
                        $('#contextMenu').menu('show',{
                            left: e.pageX,
                            top: e.pageY
                        });
                    },
                    onDrop: function (target, source, point) {
                        //lấy ra đc cái id của cái tree hiện tại
                        var targetNode = $(this).tree('getNode', target);
                        if (point === 'append') {
                            var children = [];
                            $.each(targetNode.children,
                                function (i, item) {
                                    children.push({
                                        key: item.id,
                                        value: i
                                    });
                                });

                            //update database
                            $.ajax({
                                url: '/Admin/ProductCategory/UpdateParentId',
                                type: 'post',
                                dataType: 'JSON',
                                data: {
                                    sourceId: source.id,
                                    targetId: targetNode.id,
                                    items: children
                                },
                                success: function (res) {
                                    loadData();
                                }
                            });

                        } else if (point === 'top' || point === 'bottom') {
                            //update lai order
                            $.ajax({
                                url: '/Admin/ProductCategory/ReOrder',
                                type: 'post',
                                dataType: 'JSON',
                                data: {
                                    sourceId: source.id,
                                    targetId: targetNode.id
                                },
                                success: function (res) {
                                    loadData();
                                }
                            });
                        }
                    }
                });
            },
            error: function () {

            }

        });
    }
}