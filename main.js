import { dotnet } from './_framework/dotnet.js'

const loading = document.getElementById('loading');

async function runApp() {
    try {
        const { getAssemblyExports, getConfig } = await dotnet
            .withDiagnosticTracing(false)
            .create();

        const config = getConfig();
        console.log('✓ .NET runtime created, main assembly:', config.mainAssemblyName);

        // Bind canvas before running
        dotnet.instance.Module['canvas'] = document.getElementById('canvas');
        console.log('✓ Canvas bound to module');

        // Run the .NET runtime
        await dotnet.run();
        console.log('✓ .NET runtime started');

        // Get exports after runtime is running
        const exports = await getAssemblyExports(config.mainAssemblyName);
        console.log('✓ Assembly exports loaded');

        // Hide loading screen
        if (loading) {
            loading.style.display = 'none';
        }

        // Get the UpdateFrame function
        const updateFrame = exports.PhantomNebula?.Program?.UpdateFrame;
        if (!updateFrame) {
            throw new Error('UpdateFrame not found in exports');
        }
        console.log('✓ UpdateFrame found, starting game loop');

        function mainLoop() {
            try {
                updateFrame();
            } catch (err) {
                console.error('Error in UpdateFrame:', err);
            }
            window.requestAnimationFrame(mainLoop);
        }

        window.requestAnimationFrame(mainLoop);
        console.log('✓ Game is running!');

    } catch (error) {
        console.error('Fatal error:', error);
        console.error('Stack:', error.stack);
        if (loading) {
            loading.innerHTML = `
                <div class="error">
                    <h2>Failed to Start Game</h2>
                    <p><strong>Error:</strong> ${error.message}</p>
                    <code>${error.stack}</code>
                </div>
            `;
        }
    }
}

runApp();
