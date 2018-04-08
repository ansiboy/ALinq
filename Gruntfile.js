
module.exports = function (grunt) {

    let config = {
        connect: {
            www: {
                options: {
                    // 服务器端口号
                    port: 6213,
                    // 服务器地址(可以使用主机名localhost，也能使用IP)
                    // hostname: '192.168.1.7',
                    hostname: '0.0.0.0',
                    keepalive: true,
                    // livereload: 17024,
                    // 物理路径(默认为. 即根目录) 注：使用'.'或'..'为路径的时，可能会返回403 Forbidden. 此时将该值改为相对路径 如：/grunt/reloard。
                    base: './',
                    open: true,
                    // protocol: 'https'
                }
            }
        }
    }

    grunt.initConfig(config);

    grunt.loadNpmTasks('grunt-contrib-connect');
    grunt.registerTask('default', ['connect']); //, 'copy'


}