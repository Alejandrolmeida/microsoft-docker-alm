/**!
 * AngularJS Azure blob upload service with http post and progress.
 * @author  Stephen Brannan - twitter: @kinstephen
 * @version 1.0.1
 * https://github.com/kinstephen/angular-azure-blob-upload
 */
(function () {

    var azureBlobUpload = angular.module('azureBlobUpload', []);

    azureBlobUpload.factory('azureBlob',
        ['$log', '$http', azureBlob]);

    function azureBlob($log, $http) {

        var DefaultBlockSize = 1024 * 512 // Default to 512KB

        /* config: {
          baseUrl: // baseUrl for blob file uri (i.e. http://<accountName>.blob.core.windows.net/<container>/<blobname>),
          sasToken: // Shared access signature querystring key/value prefixed with ?,
          file: // File object using the HTML5 File API,
          progress: // progress callback function,
          complete: // complete callback function,
          error: // error callback function,
          blockSize: // Use this to override the DefaultBlockSize
        } */
        var upload = function (config) {
            var state = initializeState(config);

            var reader = new FileReader();
            reader.onloadend = function (evt) {
                if (evt.target.readyState == FileReader.DONE && !state.cancelled) { // DONE == 2
                    var uri = state.fileUrl + '&comp=block&blockid=' + state.blockIds[state.blockIds.length - 1];
                    var requestData = new Uint8Array(evt.target.result);

                    $log.log(uri);
                    $http.put(uri, requestData,
                        {
                            headers: {
                                'x-ms-blob-type': 'BlockBlob',
                                'Content-Type': state.file.type,
                            },
                            transformRequest: [],
                        }).success(function (data, status, headers, config) {
                            $log.log(data);
                            $log.log(status);
                            state.bytesUploaded += requestData.length;

                            var percentComplete = (((state.bytesUploaded) / parseFloat(state.file.size)) * 100).toFixed(2);
                            
                            $(state.progress).text(percentComplete + '%');
                            $(state.progress).attr('aria-valuenow', percentComplete);
                            $(state.progress).attr('style', 'width:' + percentComplete + '%');

                            if (percentComplete == 100) {                                
                                $(state.progress).text('Completado');
                                $(state.progress).attr('class', 'progress-bar progress-bar-success progress-bar-striped');

                                state.completeAll();
                            }

                            uploadFileInBlocks(reader, state);
                        })
                        .error(function (data, status, headers, config) {
                            $log.log(data);
                            $log.log(status);

                            $(state.progress).text('Error al ' + percentComplete + '%');
                            $(state.progress).attr('class', 'progress-bar progress-bar-danger progress-bar-striped');

                            $(state.error) = 'Se ha producido un error en el envio. Status: ' + status;
                        });
                }
            };

            uploadFileInBlocks(reader, state);

            return {
                cancel: function() {
                    state.cancelled = true;
                }
            };
        };

        var initializeState = function (config) {
            var blockSize = DefaultBlockSize;
            if (config.blockSize) blockSize = config.blockSize;

            var maxBlockSize = blockSize; // Default Block Size
            var numberOfBlocks = 1;

            var file = config.file;

            var fileSize = file.size;
            if (fileSize < blockSize) {
                maxBlockSize = fileSize;
                $log.log("max block size = " + maxBlockSize);
            }

            if (fileSize % maxBlockSize == 0) {
                numberOfBlocks = fileSize / maxBlockSize;
            } else {
                numberOfBlocks = parseInt(fileSize / maxBlockSize, 10) + 1;
            }

            $log.log("total blocks = " + numberOfBlocks);

            return {
                maxBlockSize: maxBlockSize, //Each file will be split in 256 KB.
                numberOfBlocks: numberOfBlocks,
                totalBytesRemaining: fileSize,
                currentFilePointer: 0,
                blockIds: new Array(),
                blockIdPrefix: 'block-',
                bytesUploaded: 0,
                submitUri: null,
                file: file,
                baseUrl: config.baseUrl,
                sasToken: config.sasToken,
                fileUrl: config.baseUrl + config.sasToken,
                progress: config.progress,
                complete: config.complete,
                completeAll: config.completeAllBlocks,
                error: config.error,
                cancelled: false
            };
        };

        var uploadFileInBlocks = function (reader, state) {
            if (!state.cancelled) {
                if (state.totalBytesRemaining > 0) {
                    $log.log("current file pointer = " + state.currentFilePointer + " bytes read = " + state.maxBlockSize);

                    var fileContent = state.file.slice(state.currentFilePointer, state.currentFilePointer + state.maxBlockSize);
                    var blockId = state.blockIdPrefix + pad(state.blockIds.length, 6);
                    $log.log("block id = " + blockId);

                    state.blockIds.push(btoa(blockId));
                    reader.readAsArrayBuffer(fileContent);

                    state.currentFilePointer += state.maxBlockSize;
                    state.totalBytesRemaining -= state.maxBlockSize;
                    if (state.totalBytesRemaining < state.maxBlockSize) {
                        state.maxBlockSize = state.totalBytesRemaining;
                    }
                } else {
                    commitBlockList(state);
                }
            }
        };

        var commitBlockList = function (state) {
            var uri = state.fileUrl + '&comp=blocklist';
            $log.log(uri);

            var requestBody = '<?xml version="1.0" encoding="utf-8"?><BlockList>';
            for (var i = 0; i < state.blockIds.length; i++) {
                requestBody += '<Latest>' + state.blockIds[i] + '</Latest>';
            }
            requestBody += '</BlockList>';
            $log.log(requestBody);

            $http.put(uri, requestBody,
            {
                headers: {                    
                    'x-ms-blob-content-type': state.file.type,
                }
            }).success(function (data, status, headers, config) {
                $log.log(data);
                $log.log(status);
                if (state.complete) state.complete(data, status, headers, config);
            })
            .error(function (data, status, headers, config) {
                $log.log(data);
                $log.log(status);
                if (state.error) state.error(data, status, headers, config);
                // called asynchronously if an error occurs
                // or server returns response with an error status.
            });
        };

        var pad = function (number, length) {
            var str = '' + number;
            while (str.length < length) {
                str = '0' + str;
            }
            return str;
        };

        return {
            upload: upload,
        };
    };

})();
