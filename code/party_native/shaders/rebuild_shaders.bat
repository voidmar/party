fxc.exe /T vs_2_0 particle/vertex.vs /Fo vertex.vso
fxc.exe /T ps_2_0 particle/pixel.ps /Fo pixel.pso
fxc.exe /T ps_2_0 particle/pixel_mask_r.ps /Fo pixel_mask_r.pso
fxc.exe /T ps_2_0 particle/pixel_mask_g.ps /Fo pixel_mask_g.pso
fxc.exe /T ps_2_0 particle/pixel_mask_b.ps /Fo pixel_mask_b.pso
fxc.exe /T ps_2_0 particle/pixel_mask_a.ps /Fo pixel_mask_a.pso
bin2code.exe particle_shaders vertex.vso pixel.pso pixel_mask_r.pso pixel_mask_g.pso pixel_mask_b.pso pixel_mask_a.pso
del *.pso *.vso

fxc.exe /T vs_2_0 line/vertex.vs /Fo vertex.vso
fxc.exe /T ps_2_0 line/pixel.ps /Fo pixel.pso
bin2code.exe line_shaders vertex.vso pixel.pso
del *.pso *.vso